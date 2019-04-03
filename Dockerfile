# initiele dockerfile - test
FROM amsterdam/python
MAINTAINER datapunt@amsterdam.nl

ENV PYTHONUNBUFFERED 1

WORKDIR /app/
COPY requirements.txt /app/
RUN pip install -r requirements.txt

USER datapunt

CMD ["/app/docker-run.sh"]
