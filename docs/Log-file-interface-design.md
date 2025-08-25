| ??? **Field**           | **Value**                              |
|-------------------------|----------------------------------------|
| **Date**                | 2025-08-25                             |
| **Modified By**         | @copilot                               |
| **Last Modified**       | 2025-08-25                             |
| **Title**               | *Log File Interface Design*            |
| **Author**              | Logging UX                             |
| **Document ID**         | LOG-IFACE-DESIGN-001                   |
| **Document Authority**  | @KyleC69                               |
| **Version**             | 2025-08-25.v1                          |

---

### AI must always check file for update when manipulating or adding UI components. 

## Log file interface design

- Log engine will collect ALL logs from a given system (allow for enterprise expansion)
- User can supply the system with a custom log file location for collection organized by computer. Cannot assume same location on each computer in enterprise. (keep in mind enterprise expansion)
- Log engine will scan all known log locations for log files (network, system, display, services event logs, 3rd party applications etc.)
- Top half of screen will display an expandable summary of events, similar to event log summary. By severity and affected 
- Summary count of entry  severity (unknown, info, warning, error, critical)
- Bottom half will contain a chronological list of events aggregated from entire system including custom log locations.(enterprise expansion will be sortable/filtered by machine)
- Event list will be sortable by selectable list of columns. Visible list of columns will be selectable from list of available
- Event list will be able to be filtered by any combination of the following: computer, source, date/time, severity, module, ID , category
- Logging engine will poll for new log entries at a default timespan of 30 seconds. Setting controlled by policy registry key, check configsettings.md doc for specific key
- Logging engine will maintain a rolling collection of log entries in DB with a specific holding duration. Duration will be controlled by registry key, with a default of 48 hours. Key location details in config settings file

- Best practices should be followed when at all possible. Deviations should be noted in the workarounds.md document with justification and reasoning
- Interface should follow WinUI standards concerning dynamic theming and use system selected theme style (light,dark,high contrast) unless overridden by menu choice in View -> Theme

### This document is subject to change and should be tracked in doc manifest.

<!-- End Document -->