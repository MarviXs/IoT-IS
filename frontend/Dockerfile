# Base stage
FROM node:22-alpine as build-stage
# This is replaced by the env.sh script
ENV VITE_API_URL=IOT_IS_BACKEND_API_URL

WORKDIR /app
COPY package*.json ./
RUN npm install
COPY . .
RUN npm run build

# Production stage
FROM nginx:stable-alpine as production
ENV API_URL=$API_URL

COPY ./docker/nginx.conf /etc/nginx/conf.d/default.conf
COPY --from=build-stage /app/dist/spa /usr/share/nginx/html

COPY ./docker/env.sh /docker-entrypoint.d/env.sh
RUN chmod +x /docker-entrypoint.d/env.sh

CMD ["nginx", "-g", "daemon off;"]
EXPOSE 80
