FROM node:24-alpine AS builder
WORKDIR /app
COPY Frontend.App/frontend.app.client/package*.json ./
RUN npm ci
COPY Frontend.App/frontend.app.client/ ./
RUN npm run build

FROM nginx:alpine
COPY --from=builder /app/dist /usr/share/nginx/html
COPY Frontend.App/frontend.app.client/nginx.conf /etc/nginx/nginx.conf 
EXPOSE 80
CMD ["nginx", "-g", "daemon off;"]