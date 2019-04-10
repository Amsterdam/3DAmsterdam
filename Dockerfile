# initiele dockerfile - test
FROM node:10.7
RUN sed -i '/jessie-updates/d' /etc/apt/sources.list  # Now archived

MAINTAINER datapunt.ois@amsterdam.nl

EXPOSE 80

RUN apt-get update && \
  apt-get upgrade -y --no-install-recommends && \
  apt-get install -y git \
    gconf-service libasound2 libatk1.0-0 libc6 libcairo2 libcups2 libdbus-1-3 libexpat1 \
    libfontconfig1 libgcc1 libgconf-2-4 libgdk-pixbuf2.0-0 libglib2.0-0 libgtk-3-0 libnspr4 libpango-1.0-0 \
    libpangocairo-1.0-0 libstdc++6 libx11-6 libx11-xcb1 libxcb1 libxcomposite1 libxcursor1 libxdamage1 \
    libxext6 libxfixes3 libxi6 libxrandr2 libxrender1 libxss1 libxtst6 ca-certificates fonts-liberation \
    libappindicator1 libnss3 lsb-release xdg-utils \
    xvfb libgtk2.0-0 libnotify-dev libgconf-2-4 libnss3 libxss1 libasound2 && \
  rm -rf /var/lib/apt/lists/*

#COPY package.json /app/

#WORKDIR /app

ENV PATH=./node_modules/.bin/:~/node_modules/.bin/:$PATH

#COPY . /app

#RUN npm run build && cp -r /app/dist/. /var/www/html/

# forward request and error logs to docker log collector
#RUN ln -sf /dev/stdout /var/log/nginx/access.log \
# && ln -sf /dev/stderr /var/log/nginx/error.log

#COPY default.conf /etc/nginx/conf.d/
#CMD ["nginx", "-g", "daemon off;"]
