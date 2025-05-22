# Ocelot Configuration Splitting Plan

## Overview
This plan outlines how to split the monolithic Ocelot configuration into separate configuration files organized by service, using variables for common values like service URLs.

## Phase 1: Directory Structure Setup
- Create a configuration directory structure in the Gateway project
- Create separate files for each service configuration
- Create a ServiceUrls.json file to store common service URL configurations
- Implement basic configuration loading mechanism

```
Gateway/
├── Configurations/
│   ├── Auth.json
│   ├── Admin.json
│   ├── Explore.json
│   ├── Conversation.json
│   ├── ServiceUrls.json
│   └── Global.json
└── Program.cs (or Startup.cs)
```

## Phase 2: Variable-Based Configuration Implementation
- Create a ServiceUrls.json file with variable definitions for each service
- Define variables for host, port, and scheme for each service
- Implement a configuration preprocessor to replace variables in configuration files
- Ensure variable replacement works with Ocelot's configuration model

Example ServiceUrls.json:
```json
{
  "Services": {
    "Auth": {
      "Host": "authenticationservice.runasp.net",
      "Port": 80,
      "Scheme": "http"
    },
    "Admin": {
      "Host": "adminservice.runasp.net",
      "Port": 80,
      "Scheme": "http"
    },
    "Explore": {
      "Host": "exploreservice.runasp.net",
      "Port": 80,
      "Scheme": "http"
    },
    "Conversation": {
      "Host": "conversationservice.runasp.net",
      "Port": 80,
      "Scheme": "http"
    }
  }
}
```

## Phase 3: Global Configuration Setup
- Create a Global.json for global Ocelot settings
- Extract global configuration from current ocelotConfig.json
- Ensure global configuration is correctly formatted

## Phase 4: Auth Service Configuration Migration
- Extract authentication service routes from current ocelotConfig.json to Auth.json
- Update route configurations to use variables instead of hardcoded values
- Test the Auth service routes through the gateway

Example route with variables:
```json
{
  "DownstreamPathTemplate": "/api/auth/{everything}",
  "DownstreamScheme": "${Services.Auth.Scheme}",
  "DownstreamHostAndPorts": [
    {
      "Host": "${Services.Auth.Host}",
      "Port": "${Services.Auth.Port}"
    }
  ],
  "UpstreamPathTemplate": "/auth/{everything}",
  "UpstreamHttpMethod": [ "GET", "POST", "PUT", "DELETE" ]
}
```

## Phase 5: Admin Service Configuration Migration
- Extract admin service routes from current ocelotConfig.json to Admin.json
- Update route configurations to use variables instead of hardcoded values
- Test the Admin service routes through the gateway

## Phase 6: Explore Service Configuration Migration
- Extract explore service routes from current ocelotConfig.json to Explore.json
- Update route configurations to use variables instead of hardcoded values
- Test the Explore service routes through the gateway

## Phase 7: Conversation Service Configuration Migration
- Extract conversation service routes from current ocelotConfig.json to Conversation.json
- Update route configurations to use variables instead of hardcoded values
- Test the Conversation service routes through the gateway

## Phase 8: Configuration Loading Implementation
- Implement a ConfigurationLoader class to aggregate configuration files
- Add variable replacement functionality during configuration loading
- Create an extension method for IServiceCollection to add Ocelot with combined configurations
- Update Program.cs to use the new configuration loading mechanism
- Implement basic error handling for missing or invalid configuration files
- Test all routes through the gateway

## Phase 9: File Watching Implementation
- Implement a file watcher to detect changes in configuration files
- Create a service to reload configurations when files change
- Add logging for configuration reloads

## Phase 10: Advanced Features (Future)
- Add environment-specific configurations (Development, Production)
- Add configuration validation
- Add a configuration dashboard to view and edit configurations
- Add versioning for configuration files
- Add rollback capability for failed configuration changes
- Add automated tests for configuration loading