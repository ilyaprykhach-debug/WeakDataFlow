FROM node:24-alpine AS builder
WORKDIR /app
COPY frontend.app.client/package*.json ./
RUN ls -la
RUN npm ci
COPY . .
RUN npm run build

FROM nginx:alpine
COPY --from=builder /app/dist /usr/share/nginx/html
RUN ls -la
RUN find . -name "nginx.conf" -type f
RUN pwd
RUN echo "Current directory contents:"
RUN ls -la ./
RUN echo "Parent directory contents:"
RUN ls -la ../
EXPOSE 80
CMD ["nginx", "-g", "daemon off;"]