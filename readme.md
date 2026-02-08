# EveMiningFleet

**EveMiningFleet** est une plateforme web dédiée à la **gestion économique et logistique des flottes de minage** dans **EVE ONLINE**.  
Le projet fournit des outils de **centralisation, valorisation et redistribution** des minerais extraits lors d’opérations collectives.

Site public : https://eveminingfleet.ovh

---

## Finalité du projet

- Structurer économiquement les flottes de minage
- Éliminer les calculs manuels et approximatifs
- Fournir une redistribution **traçable, reproductible et vérifiable**
- Offrir une base exploitable pour corps et alliances industrielles

Le projet est conçu pour un **usage réel en production**, sur des flottes de taille moyenne à large.

---

## Fonctionnalités principales

### Gestion de flotte
- Création et administration de flottes minières
- Gestion des participants et des rôles (mineurs, logistique, défense)
- Droits d’accès configurables (lecture / écriture)

### Redistribution des minerais
- Répartition proportionnelle au volume miné
- Répartition équitable par type de minerai
- Modèles personnalisés (taxes, parts fixes, rôles non-mineurs)

### Valorisation économique
- Calcul de valeur ISK basé sur le marché
- Support des minerais compressés
- Calculs par **percentile de marché** (anti-manipulation)
- Région de référence configurable (ex. Jita)

### Outils complémentaires
- Analyse de rentabilité minière
- Aide au choix des minerais à extraire (“What I Should Mine”)
- Données exploitables pour automatisation externe

---

## Calcul des prix
- **Prix par percentile**
  - Calcul sur l’ensemble des ordres d’une région
  - Élimination des valeurs aberrantes
  - Percentile configurable (ex. 95%)

Objectif : un **prix exploitable**, stable, non biaisé par des ordres artificiels.

---

## Architecture (vue générale)

- Backend : .NET / ASP.NET
- Données marché : ESI CCP
- Base de données : MySQL
- Déploiement : Linux + Docker
- Frontend : Web (interface publique)

---

## Variables d’environnement (extrait)

```text
// Environnement d’exécution applicatif
"ENVIRONMENT": "Development",

// Chaîne de connexion base de données MySQL
// ⚠️ À ne JAMAIS versionner avec de vrais identifiants
"DB_DATA_connectionstring": "sampledataconnectionstring",
"DB_SESSION_connectionstring": "sampledataconnectionstring",

// Flag applicatif : activation du scan des prix
// 0 = désactivé, 1 = activé
"PRICESCAN": "1",

// Flag applicatif : activation du scan data du jeux pour trouver des minerais
// 0 = désactivé, 1 = activé
"ORESCAN": "1",

// Percentile utilisé pour le calcul du prix marché
"PERCENTILEPRICE": "95,0",

// Client ID OAuth EVE Online (ESI)
"EveESIClientId": "XXXXXXXXXXXXXXXXXXXXXXXX",

// Secret OAuth EVE Online
"EveESISecretKey": "XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX",

// URL de callback OAuth après authentification CCP
// Doit correspondre EXACTEMENT à celle déclarée côté développeur CCP
"EveESICallbackUrl": "https://localhost/Login/CallbackCCP"
