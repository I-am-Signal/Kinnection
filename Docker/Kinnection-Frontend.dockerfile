FROM node:20 AS build
WORKDIR /app

# Copy dependency definitions and install dependencies
COPY Kinnection-Frontend/package*.json ./
RUN npm install

# Copy app files
COPY Kinnection-Frontend .

# Build the Angular app
RUN npm run build --prod

# Build the runtime image
FROM nginx:stable-alpine AS runtime

# Copy compiled app to runtime image
COPY --from=build /app/dist/kinnection-frontend/browser /usr/share/nginx/html

# Copy NGINX config file
COPY Docker/nginx.conf /etc/nginx/templates/default.conf.template

# Set environment variables and substitute them in the config file
# Expose the port and start the app
EXPOSE ${ANG_CONTAINER_PORT}
CMD envsubst '${ANG_CONTAINER_PORT}' < /etc/nginx/templates/default.conf.template > /etc/nginx/conf.d/default.conf && nginx -g 'daemon off;'