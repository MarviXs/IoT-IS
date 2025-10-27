########################
# Deps cache (seed store)
########################
FROM node:22-alpine AS deps
ENV PNPM_HOME="/usr/local/share/pnpm" \
    PNPM_STORE_DIR="/pnpm-store" \
    PATH="$PNPM_HOME:$PATH"
RUN corepack enable && corepack use pnpm@10.19.0
WORKDIR /app

COPY pnpm-lock.yaml package.json ./
RUN --mount=type=cache,id=pnpm-store,target=/pnpm-store pnpm fetch

########################
# Build
########################
FROM node:22-alpine AS build
ENV PNPM_HOME="/usr/local/share/pnpm" \
    PNPM_STORE_DIR="/pnpm-store" \
    PATH="$PNPM_HOME:$PATH" \
    NODE_ENV=production \
    VITE_API_URL=IOT_IS_BACKEND_API_URL \
    VITE_IOT_ONLY=IOT_IS_IOT_ONLY
RUN corepack enable && corepack use pnpm@10.19.0
WORKDIR /app

COPY . .

RUN --mount=type=cache,id=pnpm-store,target=/pnpm-store \
    pnpm install --frozen-lockfile --prefer-offline

RUN pnpm run build

########################
# Runtime (Nginx)
########################
FROM nginx:stable-alpine AS production
ENV API_URL=$API_URL
COPY ./docker/nginx.conf /etc/nginx/conf.d/default.conf
COPY --from=build /app/dist/spa /usr/share/nginx/html
COPY ./docker/env.sh /docker-entrypoint.d/env.sh
RUN chmod +x /docker-entrypoint.d/env.sh
EXPOSE 80
CMD ["nginx", "-g", "daemon off;"]
