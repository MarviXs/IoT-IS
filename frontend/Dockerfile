FROM node:22-alpine as build-stage

ENV VITE_API_URL=IOT_IS_BACKEND_API_URL
ENV VITE_IOT_ONLY=IOT_IS_IOT_ONLY

WORKDIR /app
COPY package*.json ./
RUN npm install --ignore-scripts
COPY . .
RUN npm run build

FROM nginx:stable-alpine as production
ENV API_URL=$API_URL
COPY ./docker/nginx.conf /etc/nginx/conf.d/default.conf
COPY --from=build-stage /app/dist/pwa /usr/share/nginx/html
COPY ./docker/env.sh /docker-entrypoint.d/env.sh
RUN chmod +x /docker-entrypoint.d/env.sh
EXPOSE 80
CMD ["nginx", "-g", "daemon off;"]
