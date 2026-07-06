let _canvas, _ctx, _img, _naturalScale, _scale, _offsetX, _offsetY, _dragging, _lastX, _lastY;

function circleRadius() {
    return Math.min(_canvas.width, _canvas.height) / 3;
}

function draw() {
    if (!_img || !_canvas) return;
    const { width: w, height: h } = _canvas;
    _ctx.fillStyle = '#2a2a2a';
    _ctx.fillRect(0, 0, w, h);
    const iw = _img.width  * _naturalScale * _scale;
    const ih = _img.height * _naturalScale * _scale;
    _ctx.drawImage(_img, _offsetX, _offsetY, iw, ih);

    // Dark overlay over the entire canvas with circular cut-out
    const r  = circleRadius();
    const cx = w / 2, cy = h / 2;
    _ctx.fillStyle = 'rgba(0,0,0,0.75)';
    _ctx.beginPath();
    _ctx.rect(0, 0, w, h);
    _ctx.arc(cx, cy, r, 0, Math.PI * 2, true);
    _ctx.fill('evenodd');

    _ctx.strokeStyle = 'rgba(255,255,255,0.8)';
    _ctx.lineWidth = 4;
    _ctx.beginPath();
    _ctx.arc(cx, cy, r, 0, Math.PI * 2);
    _ctx.stroke();
}

// Map CSS-space coordinates to canvas buffer coordinates.
function toBufferCoords(clientX, clientY) {
    const rect = _canvas.getBoundingClientRect();
    return {
        x: (clientX - rect.left) * (_canvas.width  / rect.width),
        y: (clientY - rect.top)  * (_canvas.height / rect.height),
    };
}

export function init(canvas, imgSrc) {
    _canvas = canvas;
    _ctx    = canvas.getContext('2d');
    _scale  = 1.0;
    _dragging = false;

    canvas.width  = 500;
    canvas.height = 400;

    _img = new Image();
    _img.onload = () => {
        const diameter = circleRadius() * 2;
        _naturalScale = diameter / Math.min(_img.width, _img.height);
        _offsetX = (canvas.width  - _img.width  * _naturalScale) / 2;
        _offsetY = (canvas.height - _img.height * _naturalScale) / 2;
        draw();
    };
    _img.src = imgSrc;

    canvas.addEventListener('mousedown', e => {
        _dragging = true;
        const p = toBufferCoords(e.clientX, e.clientY);
        _lastX = p.x; _lastY = p.y;
        e.preventDefault();
    });
    canvas.addEventListener('mousemove', e => {
        if (!_dragging) return;
        const p = toBufferCoords(e.clientX, e.clientY);
        _offsetX += p.x - _lastX; _offsetY += p.y - _lastY;
        _lastX = p.x; _lastY = p.y;
        clampOffset();
        draw();
    });
    canvas.addEventListener('mouseup',    () => { _dragging = false; });
    canvas.addEventListener('mouseleave', () => { _dragging = false; });

    canvas.addEventListener('touchstart', e => {
        if (e.touches.length !== 1) return;
        _dragging = true;
        const p = toBufferCoords(e.touches[0].clientX, e.touches[0].clientY);
        _lastX = p.x; _lastY = p.y;
    }, { passive: true });

    canvas.addEventListener('touchmove', e => {
        if (!_dragging || e.touches.length !== 1) return;
        e.preventDefault();
        const p = toBufferCoords(e.touches[0].clientX, e.touches[0].clientY);
        _offsetX += p.x - _lastX; _offsetY += p.y - _lastY;
        _lastX = p.x; _lastY = p.y;
        clampOffset();
        draw();
    }, { passive: false });

    canvas.addEventListener('touchend', () => { _dragging = false; });
}

function clampOffset() {
    const iw = _img.width  * _naturalScale * _scale;
    const ih = _img.height * _naturalScale * _scale;
    const w  = _canvas.width;
    const h  = _canvas.height;
    const r  = circleRadius();
    const cx = w / 2, cy = h / 2;

    // Image must cover the circle: each edge of the image may reach the
    // corresponding circle edge but no further.
    const minX = cx + r - iw, maxX = cx - r;
    const minY = cy + r - ih, maxY = cy - r;

    _offsetX = minX <= maxX ? Math.min(maxX, Math.max(minX, _offsetX)) : (w - iw) / 2;
    _offsetY = minY <= maxY ? Math.min(maxY, Math.max(minY, _offsetY)) : (h - ih) / 2;
}

// Absolute scale relative to the natural fit — zooms toward the canvas centre.
export function setScale(scale) {
    if (!_img) return;
    const cx = _canvas.width  / 2;
    const cy = _canvas.height / 2;
    const factor = scale / _scale;
    _offsetX = cx - (cx - _offsetX) * factor;
    _offsetY = cy - (cy - _offsetY) * factor;
    _scale = scale;
    clampOffset();
    draw();
}

export function getCroppedImage(size) {
    if (!_img) return '';
    const out = document.createElement('canvas');
    out.width = size; out.height = size;
    const ctx = out.getContext('2d');

    const r  = circleRadius();
    const cx = _canvas.width  / 2;
    const cy = _canvas.height / 2;
    const ps = _naturalScale * _scale;
    const sx = (cx - r - _offsetX) / ps;
    const sy = (cy - r - _offsetY) / ps;
    const sw = r * 2 / ps;
    const sh = r * 2 / ps;
    ctx.drawImage(_img, sx, sy, sw, sh, 0, 0, size, size);

    return out.toDataURL('image/png').split(',')[1];
}
