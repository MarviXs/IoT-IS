#!/bin/bash

# Create or update ConfigMaps
kubectl create configmap emqx-init-configmap --from-file=init_user.json=../docker/emqx/init_user.json --dry-run=client -o yaml | kubectl apply -f -
kubectl create configmap emqx-cluster-configmap --from-file=cluster.hocon=../docker/emqx/cluster.hocon --dry-run=client -o yaml | kubectl apply -f -
kubectl create configmap emqx-configmap --from-file=acl.conf=../docker/emqx/acl.conf --dry-run=client -o yaml | kubectl apply -f -

kubectl apply -f configMaps/env-config.yaml

# Apply deployment and service YAML files
kubectl apply -f postgres-sts.yaml
kubectl apply -f timescaledb-sts.yaml
kubectl apply -f redis-deployment.yaml

kubectl apply -f emqx-deployment.yaml

kubectl apply -f frontend-deployment.yaml
kubectl apply -f backend-deployment.yaml

# Apply services YAML files
# kubectl apply -f internal-services.yaml
kubectl apply -f outbound-services.yaml
