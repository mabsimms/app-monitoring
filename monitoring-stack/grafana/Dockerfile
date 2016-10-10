# Grafana configuration for log ingest
FROM phusion/baseimage:0.9.15
MAINTAINER Mark Simms <masimms@microsoft.com>

ARG GRAFANA_VERSION

# Install grafana
RUN apt-get update && \
    apt-get -y --no-install-recommends install libfontconfig curl ca-certificates && \
    apt-get clean && \
    curl https://grafanarel.s3.amazonaws.com/builds/grafana_${GRAFANA_VERSION}_amd64.deb > /tmp/grafana.deb && \
    dpkg -i /tmp/grafana.deb && \
    rm /tmp/grafana.deb && \
    curl -L https://github.com/tianon/gosu/releases/download/1.7/gosu-amd64 > /usr/sbin/gosu && \
    chmod +x /usr/sbin/gosu && \
    apt-get remove -y curl && \
    apt-get autoremove -y && \
    rm -rf /var/lib/apt/lists/*

# Set up a node.js environment (for the filewatcher)
RUN apt-get update && \
    apt-get -y install npm && \
    ln -s /usr/bin/nodejs /usr/bin/node && \
    npm install -g fsmonitor

# Add the file rotate scripts
COPY rotate.sh /var/lib/grafana/rotate.sh
RUN chmod 700 /var/lib/grafana/rotate.sh

# Set up logging
RUN mkdir -p /var/log/grafana

# Set up the daemons
ADD daemons/grafana.sh /etc/service/grafana/run
ADD daemons/filemon.sh /etc/service/filemon/run

# Expose ports
EXPOSE 3000

# Grafana volumes
VOLUME ["/var/lib/grafana", "/var/lib/grafana/plugins", "/var/lib/grafana/backups", "/var/log/grafana", "/etc/grafana"]
ENV HOME /root
CMD ["/sbin/my_init"]
