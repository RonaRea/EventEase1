# EventEase Reflective Technical Report

## Introduction

EventEase is an ASP.NET Core MVC event management application designed to help booking specialists manage venues, events, and bookings in a single workflow. Across the project, the application evolved from basic CRUD screens into a cloud-connected solution that supports Azure SQL Database for operational data, Azure Blob Storage for image handling, and Azure App Service for hosting.

## Full Feature List

The completed application includes the following features:

- venue management with create, read, update, and delete flows
- event management with image upload, date validation, and event classification
- booking management that links one event to one venue
- event search by keyword, date range, booking status, event type, and venue availability
- venue filtering by keyword, capacity, and availability
- conflict detection that blocks overlapping bookings for the same venue
- automatic database migration on application startup
- Azure Blob Storage integration for venue and event images
- Azure App Service deployment workflow through GitHub Actions
- SQL view support for booking overview reporting

## Azure Service Discussion

### Azure App Service

Azure App Service was used to host the web application because it fits the architecture of a server-rendered ASP.NET Core MVC application. It reduces infrastructure overhead, supports continuous deployment, handles HTTPS and scaling features, and provides an efficient way to expose the application publicly.

An alternative could have been Azure Container Apps or Azure Kubernetes Service if container-level control, microservices, or advanced scaling policies were required. For this project, App Service was the simpler and more appropriate choice.

### Azure SQL Database

Azure SQL Database was used to store the relational data for venues, events, bookings, and reporting views. The project’s data model depends on structured relationships, foreign keys, validation rules, and transactional updates. Those requirements align closely with a relational database platform.

An alternative could have been Azure Cosmos DB, but that would have required a different data model and different query patterns. For tightly related operational entities such as event bookings and venue assignments, Azure SQL Database provided a better fit.

### Azure Blob Storage

Azure Blob Storage was used to store uploaded venue and event images. This prevented the application from relying on the local file system and made media storage more scalable and cloud-friendly. Blob Storage is also cost-effective for large binary objects and simplifies public or controlled access to uploaded content.

An alternative could have been storing images directly in the database, but that would increase database size and reduce performance efficiency for relational workloads. Blob Storage is the more appropriate design for static media.

## Technologies Used And Why

- ASP.NET Core MVC was used because it supports clear separation of concerns through controllers, models, and views.
- Entity Framework Core was used to model the database, manage migrations, and simplify data access.
- Razor Views were used to produce server-rendered UI pages with tight integration to model binding and validation.
- SQL Server was used because it supports relational integrity, indexes, views, and strong reporting patterns.
- GitHub Actions was used to automate deployment to Azure App Service.
- Azure Storage SDK was used to upload and manage image files in Blob Storage.

These technologies worked well together because the project required a conventional web application architecture with relational data, cloud deployment, and media handling.

## Reflection On Design, Development, And Deployment

From Part 1 to the current stage, the biggest learning point was that cloud application development is not only about building features. It also depends on designing data correctly, planning how services interact, and preparing the application for deployment and maintenance.

During development, I learned that relational modelling matters because small design decisions affect search, filtering, validation, and reporting later. Adding the `EventType` lookup and the `IsAvailable` venue field showed how new business rules often require coordinated changes across models, migrations, UI forms, controllers, and reporting queries.

I also developed a stronger understanding of defensive application logic. Booking validation could not rely only on the user interface; it needed controller and database-aware checks to prevent double-booking and to stop unavailable venues from being assigned.

On the cloud side, I learned that Azure services should be chosen by responsibility. Azure SQL Database handled structured application data, Blob Storage handled unstructured images, and App Service handled hosting. This separation made the architecture easier to reason about and easier to maintain.

The project also highlighted that deployment is part of software engineering, not a final afterthought. Migrations, environment configuration, secure secrets handling, and deployment automation all influence whether a project is production-ready.

## Theory Discussion

### How Cosmos DB Differs From Traditional Databases

Azure Cosmos DB differs from traditional relational databases because it is designed as a globally distributed, horizontally scalable NoSQL platform. Traditional databases such as SQL Server depend on structured tables, fixed schemas, joins, and strong relational modelling. Cosmos DB is more flexible for semi-structured or rapidly changing data because it commonly stores JSON-like documents and scales through partitioning.

Traditional relational databases are usually the better choice when the solution needs strong joins, transactions across related entities, strict consistency in a single region, and well-defined reporting structures. Cosmos DB is more useful when the system needs global distribution, very low latency, elastic scale, and high throughput across very large workloads.

### Key Considerations For Logic Apps Handling Sensitive Data

When designing Logic Apps that process sensitive data, the main concerns are security, compliance, and exposure minimisation. Sensitive inputs and outputs should be protected, secrets should be stored in secure services such as Azure Key Vault, and access should follow least privilege. Designers should also think carefully about connector permissions, logging, retry behavior, and whether data could be exposed in run history or diagnostics.

It is also important to encrypt data in transit and at rest, limit who can view workflow runs, and use managed identity where possible. If sensitive information is processed, the workflow should be designed to minimise unnecessary data movement between systems.

### Combining Event Grid With Other Services For Robust Workflows

Azure Event Grid becomes especially powerful when paired with services that react to events. For example, Event Grid can detect that a blob was uploaded, then trigger Azure Functions for processing, Logic Apps for notifications, or Service Bus for more controlled downstream handling.

This creates robust workflows because systems become event-driven rather than tightly coupled. Services can react independently, failures can be isolated, and new subscribers can be added without rewriting the original publisher. In a broader EventEase-style architecture, Event Grid could notify downstream components when a new image is uploaded, when a booking is created, or when an event changes state.

## Conclusion

This project built both application development skills and cloud architecture awareness. The main outcome was not only a functional event management application, but also a clearer understanding of how Azure services, database design, deployment automation, and validation logic work together to produce a reliable cloud-based solution.