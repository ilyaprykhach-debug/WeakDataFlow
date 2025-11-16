FROM node:24-alpine AS builder
WORKDIR /app

ARG VITE_GRAPHQL_URI=http://localhost:5004/graphql
ARG VITE_NOTIFICATION_URI=http://localhost:5003/notificationHub

ENV VITE_GRAPHQL_URI=$VITE_GRAPHQL_URI
ENV VITE_NOTIFICATION_URI=$VITE_NOTIFICATION_URI

COPY Frontend.App/frontend.app.client/package*.json ./
RUN npm ci
COPY Frontend.App/frontend.app.client/ ./
RUN npm run build

FROM nginx:alpine
COPY --from=builder /app/dist /usr/share/nginx/html
EXPOSE 80
CMD ["nginx", "-g", "daemon off;"]