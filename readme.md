# EveMiningFleet – EVE Partner

**EveMiningFleet** is a web platform dedicated to the **economic and logistical management of mining fleets** in **EVE ONLINE**.  
The project provides tools for **centralization, valuation, and redistribution** of ores extracted during collective operations.

Public site: https://eveminingfleet.ovh

---

## Project Purpose

- Economically structure mining fleets  
- Eliminate manual and approximate calculations  
- Provide **traceable, reproducible, and verifiable** redistribution  
- Offer an exploitable foundation for industrial corporations and alliances  

The project is designed for **real production use**, targeting medium to large fleets.

---

## Main Features

### Fleet Management
- Creation and administration of mining fleets  
- Participant management  

### Ore Redistribution
- Proportional redistribution based on mined volume  
- Fair redistribution based on volume and additional parameters  
- Custom models (taxes, fixed shares, non-mining roles)  

### Economic Valuation
- ISK value calculation based directly on EVE Online market data  
- Reference market: Jita 4–4 (Moon 4 – Caldari Navy Assembly Plant)  
- Support for compressed ores  
- **Market percentile–based** calculations (anti-manipulation)  

### Additional Tools
- Moon mining profitability analysis, including potential taxation using EVE Online moon ledger data  
- Ore selection assistance (“What I Should Mine”)  
---

## Price Calculation

**Percentile-based pricing**
- Calculation over all market orders in a region  
- Removal of outlier values  
- Configurable percentile (e.g. 95%)  

**Objective:** provide an **actionable**, stable price, not biased by artificial or manipulative orders.

---

## Architecture (High-Level)

- **Backend:** .NET / ASP.NET  
- **Market Data:** CCP ESI  
- **Database:** MySQL  
- **Deployment:** Linux + Docker  
- **Frontend:** Web (public interface)  

---

## Environment Variables (Excerpt)

```text
// Application runtime environment
"ENVIRONMENT": "Development",

// MySQL database connection string
// ⚠️ Must NEVER be versioned with real credentials
"DB_DATA_connectionstring": "sampledataconnectionstring",
"DB_SESSION_connectionstring": "sampledataconnectionstring",

// Application flag: enable price scanning
// 0 = disabled, 1 = enabled
"PRICESCAN": "1",

// Application flag: enable in-game data scan to discover ores
// 0 = disabled, 1 = enabled
"ORESCAN": "1",

// Percentile used for market price calculation
"PERCENTILEPRICE": "95,0",

// EVE Online OAuth Client ID (ESI)
"EveESIClientId": "XXXXXXXXXXXXXXXXXXXXXXXX",

// EVE Online OAuth Secret
"EveESISecretKey": "XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX",

// OAuth callback URL after CCP authentication
// Must EXACTLY match the one declared on the CCP developer side
"EveESICallbackUrl": "https://localhost/Login/CallbackCCP"
