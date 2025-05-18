# Project Onboarding Prompt
When onboarding an existing project, I will follow a structured approach to ensure that the project is well-integrated. This includes creating a comprehensive directory structure, analyzing the project, discovering existing documentation, extracting core components, creating initial specifications, generating a knowledge base, and setting up task tracking.

When run this prompts, you will execute all the steps in the order listed below.

## Steps for Onboarding the project

Creating the required directory structure:

```
.project/
├── knowledge/
│   └── project_overview.md
├── epics/
│   ├── epic_001.md
│   ├── epic_001_user_stories/
│   │   ├── epic_001_us_001.md
│   │   ├── epic_001_us_002.md
│   │   └── ...
│   ├── epic_002.md
│   ├── epic_002_user_stories/
│   │   └── ...
│   └── ...
├── tasks/
│   ├── task_001.md
│   ├── task_002.md
│   └── ...
```

## Example Content

### .project/knowledge/project_overview.md
```markdown
# Project Overview

## Context
Describe the business context and main goals.

## Architecture
- Clean Architecture
- DDD principles
- Deployment: Monolith, Monorepo, or Microservices (adapt as needed)

## Main Flows
- Order creation with product availability check
- Event publishing to Kafka
- Order status update via event listener
```

### .project/epics/epic_001.md
```markdown
# Epic 001: Order Management

## Description
As a user, I want to create and manage orders so that I can purchase products.
```

### .project/epics/epic_001_user_stories/epic_001_us_001.md
```markdown
# User Story 001

**As a** [role or persona]
**I want** [to perform an action or achieve a goal]
**So that** [I get a specific value or benefit]

**Status**: [Draft | In Progress | Done]
**Progress**: [0%]

## 3 Amigos Session
- Participants: PO, Dev, QA
- Date: 2025-05-18
- Notes: Critères d’acceptance validés ensemble

## Acceptance Criteria (Given-When-Then)

- **Scenario 1: [Short scenario title]**
  - **Given** [context or precondition]
  - **When** [action or event]
  - **Then** [expected outcome]

- **Scenario 2: ...**
  - **Given** ...
  - **When** ...
  - **Then** ...

## Linked Tasks
- [task_001](../../../tasks/task_001.md)
```

### .project/tasks/task_001.md
```markdown
# Tasks for User Story [XXX]

**User Story Reference**  
- [epic_XXX_us_XXX](../epics/epic_XXX_user_stories/epic_XXX_us_XXX.md)

## Task List
- [ ] Task 1: [Short, action-oriented description]
- [ ] Task 2: [Short, action-oriented description]
- [ ] Task 3: [Short, action-oriented description]

## Files to Create or Modify
- [ ] /path/to/file1.ext (new or modify)
- [ ] /path/to/file2.ext (new or modify)
- [ ] /path/to/file3.ext (new or modify)

## How to Test (TDD Notes)
- [ ] What is the expected behavior for each task?
- [ ] What unit or integration tests should be written first?
- [ ] What are the acceptance criteria for each test?
- [ ] What tools or frameworks will be used for testing?

**Context**  
[Optional: Briefly explain why these tasks are needed or what they relate to]

**Acceptance Criteria (Given-When-Then)**

- **Scenario 1: [Short scenario title]**
  - **Given** [context or precondition]
  - **When** [action or event]
  - **Then** [expected outcome]

- **Scenario 2: ...**
  - **Given** ...
  - **When** ...
  - **Then** ...
```