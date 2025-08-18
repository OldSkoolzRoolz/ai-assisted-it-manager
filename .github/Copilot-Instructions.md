## Copilot Template for AI assisted Policy & System Management Software Design


## Folder Structure

- `/src`: Contains the project folders, each folder is a project module
- `/docs`: Contains architecture outline, Implementation phases documentation for the project, including API specifications and user guides.
- `/src/ClientApp`: Contains the individual client project.
- `/src/CorePolicyEngine`: Contains the policy control project.
- `/src/EnterpriseDashboard`: Enterprise control interface project. For future paid versions
- `/src/Security`: Security project for controlling application and enterprise security
- `/src/AICompanion`: Contains project for the AI Companion document

## Project Key features include:
- Ability to parse, validate, and deploy ADMX policies and apply then to the local machine; Enterprise level features for paid version.
- AI Companion: Companion should have ability to monitor key areas such as event logs, application logs, registry changes and other patterns to identify possible security problems or system configuration problems to alert IT.
- AI powered recommendations for policy changes based on system state and historical data.
- AI powered problem solver to assist IT in troubleshooting issues related to policy application and system configuration; Make recommendations based on historical data, best practices, known antipatterns, and system state.
- Enterprise Dashboard: A future Blazor based dashboard to provide a unified view of system management and policy application, system state, and AI recommendations, alerts, logs, Etc.
- Security: TLS, signing, auditing, and other security measures to ensure the integrity and confidentiality of the system. Given the inherent power of this application and the potential for misuse, security is a top priority. This includes secure communication, data integrity, and auditing capabilities to track changes and access to the system. Ensure all modules are secure and follow best practices for security and compliance.
- Modular Architecture: The system should be modular and extensible, allowing for easy addition of new features and capabilities. This includes a clear separation of concerns, with well-defined interfaces and abstractions to allow for easy integration of new components. Modules must be standalone and not coupled to any other module. They will be part of pay-per-feature model. Separation of purpose is paramount to ensure that each module can be developed, tested, and deployed independently. This will also allow for easier maintenance and updates in the future.
- IT System Management: The system should provide a comprehensive set of tools for managing IT systems, including policy management, system monitoring, and troubleshooting. This includes the ability to manage policies across multiple systems, monitor system state, and troubleshoot issues related to policy application and system configuration.

## Key Concepts
- **Enterprise Context**: All enterprise type functionality will be in a paid release. Limit visibility to enterprise features in the free version. Exact mechanism for controlling free/paid version to be determained later. Maintain a clear separation of concerns for easy implementation of this.
- **Policy Management**: It is imperative to maintain a clear and concise trail of each policy change, including who made the change, when it was made, and what the change was. This is crucial for compliance and auditing purposes. The ability to revert changes and view historical versions of policies is essential. Policies should be versioned and stored in a way that allows for easy retrieval and comparison of changes over time. Policies are always changing as new venders and system requirements emerge. The system should be able to handle these changes gracefully, allowing for easy updates and modifications to existing policies without losing historical context. AI Companion feature must monitor published policy schemas and adapt models accordingly.
- **AI Companion**: The AI Companion should be able to monitor key areas such as event logs, application logs, registry changes, and other patterns to identify possible security problems or system configuration problems to alert IT. It should also be able to provide recommendations for policy changes based on system state and historical data. The AI Companion should be able to learn from past interactions and improve its recommendations over time.
- **Modular Architecture**: The system should be modular and extensible, allowing for easy addition of new features and capabilities. Each module should be self-contained with clear interfaces; no direct dependencies between modules. This will allow for easier development and testing. Development of new modules should not require changes to existing modules. Modules must be standalone and not coupled to any other module. They will be part of pay-per-feature model.
- **AI Interface**: The AI interface will be able to suggest actions in response to user questions and requests. Example: "Are there any system errors reported in the last 24 hours?" The AI will respond with a list of errors, their severity, and recommendations for resolution. The AI will also be able to provide recommendations for policy changes based on system state and historical data. The AI will be able to learn from past interactions and improve its recommendations over time. AI will also learn from reputable community self-help sites such as StackOverflow and GitHub issues to provide relevant solutions and workarounds for common problems.

## Current Modules and their responsibilities
- **Core.Engine**: The core engine will handle the parsing, validation, and deployment of ADMX policies. It is responsible for the retrieval, parsing, manipulation and deployment of ADMX policies. 
- **AICompanion**: Provides AI powered chat interface for user interaction, monitoring system state, and providing recommendations for policy changes and troubleshooting. It will also handle the AI powered problem solver to assist IT in troubleshooting issues related to policy application and system configuration.
- **ClientApp**: The client application will provide the user interface for the local system. It will be a WPF application that will allow users to interact with the system, view policies, and manage system state. The UI will be designed to be intuitive and easy to use, with a focus on providing a seamless user experience.
- **EnterpriseDashboard**: A future Blazor based dashboard to provide a unified view of system management and policy application, system state, and AI recommendations, alerts, logs, etc. This will be a web-based application that will allow users to manage policies across multiple systems and view system state in real-time.
- **SecurityModule**: This module will handle all security related functionality, including TLS, signing, auditing, and other security measures to ensure the integrity and confidentiality of the system. It will also handle user authentication and authorization, ensuring that only authorized users can access sensitive data and perform actions that could compromise system integrity.

## Coding Conventions
- C#: PascalCase for public members, _camelCase underscore tolerant for private fields. Use file-scoped namespaces.
- Prefer async suffix for async methods; cancellation tokens on all IO boundaries.
- Use dependency injection; no static singletons (except pure helpers).
- If working with complex or expensive operations, create metric counters that can be monitored.
- Logging: Use high perorformance structured logging per Microsoft standards using ILogger<T> patterns. Use log levels appropriately.
- Maintain separation of concerns; create directories to separate functional areas (e.g., Core.Engine, Core.Validation, Core.Deployment, Core.Parsing). Namespaces to follow directory patterns.
- Use explicit interfaces for all services; no direct instantiation of concrete classes; Abstract factories for service creation.
- Localization: Use resource files for all user-visible strings; no hardcoded strings in code.
- Import statements should be outside of namespace

## Architecture Constraints
- UI must NOT access registry or COM directly; route via services / broker.
- Core parsing is pure (no side effects) except explicit I/O loads.
- Modular design: Each module must be self-contained with clear interfaces; no direct dependencies between modules. This will allow for easier develpment and testing. Development of new modules should not require changes to existing modules.
- API documentation is a must and AI generated by Github Copilot to ensure consistency and clarity.
- Versioning service module is responsible for all versioning tracking for application modules, database schema and policy schema changes.
- Use interfaces for all services; no direct instantiation of concrete classes; Abstract factories for service creation.
- All module/projects must compile with .NET 9.0 SDK; no legacy .NET Framework code.
- Use explicitly defined imports for dependencies; no implicit imports. Makes it easier to manage dependencies and avoid conflicts. Use explicit versioning for all dependencies to ensure compatibility and stability.
- UI will use WPF until the SDK is fixed and WinUI is more stable. UI may be converted in later phases.
- UI Interface must be secure and actions prevented if the user does not have sufficient permissions to perform the action. This includes preventing access to sensitive data and actions that could compromise system integrity.

## 5. Data model characteristics to maintain
- Aligned with domain language (ubiquitous terms, clear bounded contexts)
- Clear entity responsibilities (no God objects, high cohesion, low coupling)
- Appropriate normalization (3NF-ish for OLTP; deliberate denormalization only for performance/read models)
- Explicit relationships (cardinality + ownership + delete behaviors defined)
- Strong integrity rules (constraints, foreign keys, unique indexes, check constraints)
- Consistent, descriptive naming (singular tables, predictable key patterns)
- Stable primary keys (non-meaningful, immutable; avoid natural keys unless truly stable)
- Versionability & extensibility (migration friendly, backward-compatible changes planned)
- Performance aware (indexed for critical queries, measured not guessed)
- Minimal redundancy (no duplicated facts without reconciliation strategy)
- Clear nullability semantics (null means  unknown/not applicable  only)
- Temporal/audit support where needed (created/modified, soft delete, history tables)
- Security & privacy embedded (row/column classification, least privilege, PII isolation)
- Data quality validation (at ingest boundaries, not just in app logic)
- Separation of read/write concerns when necessary (CQRS, materialized views)
- Traceability & lineage (reference sources for derived data)
- Testable (seed scripts, deterministic fixtures)
- Documentation adjacent (ERD, glossary, change log)

## Data handling guidelines
- Use SQLite for all data persistence; no direct file I/O outside of SQLite context.
- Maintain secure handling of all data, especially sensitive information.
- Database migrations must be versioned, packaged and tracable. ensure that database schema changes are versioned and tracable by module so that module vesions can be paired with database schema versions for easy management.
- Use migrations to handle schema changes, ensuring backward compatibility where possible.
- Generate stored procedures for ease of versioning and deployment.

## AI Prompts and Responses
- Code generation prompts should be clear and concise, specifying the desired functionality, input/output requirements, and any constraints.
- Responses should be structured, with clear separation of code, comments, and explanations.
- If public code repositories are used for reference ensure they are recent, well-maintained, and follow best practices.
- Provide explanations for key decisions, especially for complex logic or algorithms.

##  Testing Guidance
- Unit testing will be done using xUnit; use Moq for mocking dependencies. Unit test will be design at a later phase. When generating code, keep in mind that unit tests will be required for all public methods and properties so design code to be testable.

##  Security & Compliance Notes
- Never embed credentials, cert private keys, or domain specifics in code or prompts.
- Ensure paths and file operations validate input (no directory traversal).
- Plan for TLS + signing but mock in early phases.

## Ensure Clear and Concise Prompts
- If requests are too large or complex, prompt user for smaller parts and make suggestions as to how to break it down.
- If the request is too vague, ask for more details.
- if the request seems to violate the guidlines in this document, notify user, ask for clarification and suggest alternatives.
- 


## Avoid
- Generating large monolithic classes (>300 lines)   ask for slices (e.g., "only diff logic").
- Mixing layers (no UI types inside Core.Engine).
- Direct registry manipulation from tests without abstraction.




---
Keep this file concise; expand only when a repeated clarification emerges. Update alongside architecture changes.
Version: 2.0
Date: 08-17-2025
