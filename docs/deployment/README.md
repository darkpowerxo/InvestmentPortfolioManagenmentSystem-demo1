# Deployment Documentation

This directory contains comprehensive deployment guides for the CDPQ Investment Portfolio Management System.

## Available Deployment Options

### 🐳 Docker Deployment
**File**: `docker-deployment.md`

Complete guide for deploying using Docker containers with Docker Compose. Includes both development and production configurations with monitoring, SSL, and high availability setup.

**Quick Start**:
```bash
# Development
docker-compose up -d

# Production
docker-compose -f docker-compose.prod.yml up -d
```

### ☸️ Kubernetes Deployment
**File**: `kubernetes-deployment.md`

Enterprise-grade Kubernetes deployment with Kustomize for configuration management. Includes autoscaling, network policies, and environment-specific overlays.

**Quick Start**:
```bash
# Development
kubectl apply -k k8s/overlays/development

# Production
kubectl apply -k k8s/overlays/production
```

### 🏗️ Architecture Diagrams
**File**: `deployment-architecture.md`

High-level deployment architecture documentation with detailed diagrams and infrastructure requirements for CDPQ production environment.

## Deployment Files Structure

```
├── docs/deployment/
│   ├── README.md                     # This file
│   ├── deployment-architecture.md    # Architecture overview
│   ├── docker-deployment.md          # Docker deployment guide
│   └── kubernetes-deployment.md      # Kubernetes deployment guide
├── docker-compose.yml                # Development Docker setup
├── docker-compose.prod.yml           # Production Docker setup
├── Dockerfile                        # Container build instructions
├── .env.example                      # Development environment template
├── .env.production.example           # Production environment template
└── k8s/                              # Kubernetes manifests
    ├── base/                         # Base Kubernetes resources
    └── overlays/                     # Environment-specific customizations
        ├── development/              # Development environment
        └── production/               # Production environment
```

## Quick Deployment Summary

| Environment | Method | Command | Access |
|-------------|--------|---------|---------|
| Development | Docker | `docker-compose up -d` | http://localhost:5000 |
| Development | Kubernetes | `kubectl apply -k k8s/overlays/development` | dev-api.portfolio.cdpq.com |
| Production | Docker | `docker-compose -f docker-compose.prod.yml up -d` | https://portfolio.cdpq.com |
| Production | Kubernetes | `kubectl apply -k k8s/overlays/production` | https://api.portfolio.cdpq.com |

## Prerequisites

- **Docker**: Engine 20.10+, Compose 2.0+
- **Kubernetes**: v1.20+, kubectl, kustomize
- **Resources**: Minimum 8GB RAM, 4 CPU cores for development
- **Network**: Ports 80/443 available for web traffic

## Support

For deployment issues or questions:

1. Check the specific deployment guide for your chosen method
2. Review the troubleshooting sections in each guide
3. Verify system prerequisites are met
4. Check application logs and health endpoints

## Next Steps

1. Choose your deployment method (Docker or Kubernetes)
2. Review the corresponding deployment guide
3. Prepare your environment and prerequisites
4. Follow the step-by-step deployment instructions
5. Verify the deployment using the provided health checks