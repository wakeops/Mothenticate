<p align="center">
    <img alt="mothentik" src="assets/brand_logo.svg" height="150" >
</p>

---

[![GitHub Workflow Status](https://img.shields.io/github/actions/workflow/status/wakeops/mothenticate/ci-main.yml?branch=main&style=for-the-badge)](https://github.com/wakeops/mothenticate/actions/workflows/ci-main.yml)
![Latest version](https://img.shields.io/docker/v/mothenticate/mothenticate?sort=semver&style=for-the-badge)
[![MIT License](https://img.shields.io/badge/License-MIT-green?style=for-the-badge)](LICENSE)

## Mothenticate

Mothenticate is an open source IdP for SSO. It supports OAuth2/OIDC and is designed for small scale levels of usage.

## Installation

### Docker Compose

```yaml
services:
  mothenticate:
    image: mothenticate/mothenticate:latest
    restart: unless-stopped
    depends_on:
      - postgres
    environment:
      ISSUER_URI: auth.example.com
      POSTGRESQL__HOST: postgres
      POSTGRESQL__NAME: mothenticate
      POSTGRESQL__USER: mothenticate
      POSTGRESQL__PASSWORD: changeme
    ports:
      - "5000:5000"

  postgres:
      image: postgres:16-alpine
      restart: unless-stopped
      environment:
        POSTGRES_DB: mothenticate
        POSTGRES_USER: mothenticate
        POSTGRES_PASSWORD: changeme
      ports:
        - "5432:5432"
      volumes:
        - postgres_data:/var/lib/postgresql/data

volumes:
  postgres_data:
```

`ISSUER_URI` must match the public URL Mothenticate is reachable at — this is used in OIDC token issuance and discovery.

## Screenshots

*Coming Soon*

## How to contribute

**Not accepting contributions at this time**
