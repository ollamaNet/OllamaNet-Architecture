# Ocelot Configuration Splitting Plan

## Overview
This plan outlines how to split the monolithic Ocelot configuration into separate configuration files organized by service.

## Phase 1: Directory Structure Setup
- Create a configuration directory structure in the Gateway project
- Create separate files for each service configuration
- Implement basic configuration loading mechanism

```
Gateway/
├── Configurations/
│   ├── Auth.json
│   ├── Admin.json
│   ├── Explore.json
│   ├── Conversation.json
│   └── Global.json
└── Program.cs (or Startup.cs)
```

## Phase 2: Configuration Splitting
- Extract routes from current ocelotConfig.json into separate service-specific files
- Move authentication service routes to Auth.json
- Move admin service routes to Admin.json
- Move explore service routes to Explore.json
- Move conversation service routes to Conversation.json
- Create a Global.json for global Ocelot settings
- Ensure all configurations are correctly formatted

## Phase 3: Configuration Loading Implementation
- Implement a ConfigurationLoader class to aggregate configuration files
- Create an extension method for IServiceCollection to add Ocelot with combined configurations
- Update Program.cs to use the new configuration loading mechanism
- Implement basic error handling for missing or invalid configuration files

## Phase 4: File Watching Implementation
- Implement a file watcher to detect changes in configuration files
- Create a service to reload configurations when files change
- Add logging for configuration reloads

## Phase 5: Advanced Features (Future)
- Add environment-specific configurations (Development, Production)
- Add configuration validation
- Add a configuration dashboard to view and edit configurations
- Add versioning for configuration files
- Add rollback capability for failed configuration changes
- Add automated tests for configuration loading 