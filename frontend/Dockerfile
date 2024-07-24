FROM node:lts-alpine3.19 AS build-stage
ARG API_URL=${API_URL}
ENV API_URL=${API_URL}
WORKDIR /app
COPY package*.json ./
RUN npm install
COPY . .
RUN npm run build

FROM steebchen/nginx-spa:stable AS runtime-stage
COPY --from=build-stage /app/dist/spa /app
EXPOSE 80
CMD ["nginx"]
