# Base stage
FROM node:22-alpine as base

ARG VITE_API_URL
ENV VITE_API_URL=$VITE_API_URL

ENV NODE_ENV ${BUILD_ENV}
WORKDIR /app
COPY package*.json ./
RUN npm install

# Build stage for production
FROM base as build-stage
COPY . .
RUN npm run build

# Production stage
FROM nginx:stable-alpine as production
COPY ./docker/nginx.conf /etc/nginx/conf.d/default.conf
COPY --from=build-stage /app/dist /usr/share/nginx/html
CMD ["nginx", "-g", "daemon off;"]
EXPOSE 8080
