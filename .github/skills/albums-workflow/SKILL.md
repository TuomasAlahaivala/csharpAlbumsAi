---
name: albums-workflow
description: "Run the multi-agent implementation workflow for a feature or bug fix: coding, independent review, and testing."
disable-model-invocation: true
argument-hint: "Describe the feature or bug fix and its acceptance criteria"
---
Run the complete multi-agent workflow for this request:

${input:task:Describe the feature or bug fix and its acceptance criteria}

Follow the required sequence: delegate implementation to Albums Coder, independent review to Albums Reviewer, and verification to Albums Tester. Resolve actionable review findings and change-caused test failures through the coder, then repeat review and verification as needed. Finish with a concise report of changes, review findings, exact checks run, results, and blockers. Do not commit, push, or deploy unless explicitly requested.
