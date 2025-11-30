# MiddleWareProject (Let's go Biking)

## Environment Configuration (`.env` File)

Some services in this project require external API keys.
To provide them, you **must create a `.env` file at the root of the project** (next to `docker-compose.yml`).

Create a file named:

```
.env
```

and add the following content:

```
JcDecauxApiKey=jcdecaux_apikey
OpenRouteApiKey=openroute_apikey
```

### Meaning of each key

| Variable            | Description                                                                     |
| ------------------- | ------------------------------------------------------------------------------- |
| **JcDecauxApiKey**  | API key used to request bike availability data from the **JCDecaux Bike API**.  |
| **OpenRouteApiKey** | API key used to communicate with **OpenRouteService** for routing computations. |

### Notes

* These values are **mandatory** for the Proxy Cache Server and Routing Server to work properly.
* Replace `jcdecaux_apikey` and `openroute_apikey` with your **real** keys.
* The `.env` file is automatically loaded by Docker Compose and passed to the relevant containers.

---

## How to run the app

### 1. Deploying the Docker environment

Make sure **Docker** and **Docker Compose** are installed on your system.

To start all services:

```bash
docker-compose up --build
```

Once everything is running, open the web application at:

**[http://localhost:80](http://localhost:80)**

---

### 2. Building and running the Heavy Client (SOAP Client)

The Heavy Client is a .NET SOAP-based desktop/CLI application used to interact with the SOAP interface of the Proxy Cache Server.

To build and run it:
```bash
cd backend/HeavyClient
dotnet build
dotnet run
```

You may also open the project in **Visual Studio** and run it from there.

The HeavyClient communicates exclusively via SOAP with the Proxy Cache server.

---

## How to use the web app

When you access **[http://localhost:80](http://localhost:80)**, you can:

* **Compute an itinerary** by entering a departure and destination address
* **Visualize the computed route** on a map
* **See detailed steps** on the left panel
* **View itinerary details**, calculated by the Routing Server (with optional bike-sharing legs)
* **Connect to the ActiveMQ notification broker**
* **Select the topics you want to subscribe to** (weather, pollution, traffic, etc.)
* **Receive real-time notifications** produced by the Notification Publisher service through ActiveMQ/STOMP

---

## Architecture Overview

This project implements a distributed middleware-based ecosystem composed of several coordinated microservices.
Each component contributes to routing, real-time data broadcasting, caching, or user interaction.

### **Main Components**

#### **1. Frontend (Vite + Nginx)**

* Displays the interactive map (Leaflet)
* Communicates with backend services (Routing, Proxy Cache)
* Connects to ActiveMQ WebSocket endpoint via STOMP for notifications
* Lets the user subscribe to topics and view messages in real time

#### **2. Routing Server**

* Computes multimodal itineraries (walking + bike-sharing + walking)
* Aggregates data from OpenRouteService and the JCDecaux API
* Returns GeoJSON used by the frontend to display the itinerary

#### **3. Proxy Cache Server**

* Acts as a cache layer for external API requests (OpenRouteService, JCDecaux API)
* Reduces latency and external API call volume
* Prevents rate-limit issues
* HeavyClient uses it to request routing and address data via SOAP

#### **4. ActiveMQ Broker**

* Central messaging system
* Exposes both:

  * **STOMP/WebSocket (ws://activemq:61614)** for the frontend
  * **STOMP TCP (61613)** for backend services
* Hosts multiple topics:

  * `/topic/weather`
  * `/topic/pollution`
  * `/topic/bikeAvailability`
  * `/topic/traffic`
  * `/topic/maintenance`
  * `/topic/safety.alerts`

#### **5. Notification Publisher**

* Periodically generates random or simulated notifications
* Publishes them to ActiveMQ topics
* Feeds the frontend with real-time updates

#### **6. Heavy Client (CLI)**

* A .NET interactive command-line interface
* Classic “fat” client
* Talks exclusively to the Proxy Cache Server via SOAP
* Can:
  * Search addresses
  * Request routing
  * Retrieve cached data
* Demonstrates traditional enterprise middleware interaction patterns

---

## Summary

This project demonstrates:

* Modern microservice orchestration with Docker
* A hybrid **REST + SOAP** backend architecture
* Frontend real-time communication via **ActiveMQ STOMP WebSocket**
* A caching middleware layer for performance optimization
* A routing engine integrating multiple external data sources
* A traditional heavy client consuming SOAP services
* Event-driven architecture using a message broker

---
