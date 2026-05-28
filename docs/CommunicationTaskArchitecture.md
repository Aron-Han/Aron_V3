# Communication And Task Architecture

## Current Principle

Task is the top-level business requirement. A task represents one asynchronous inspection or workflow execution. Communication protocols are not above tasks; they are shared runtime resources that tasks can use.

## Target Shape

- Communication connections are long-lived instances, such as `TCPIP_Server_3000`, `TCPIP_Client_PLC`, `S7_Station_A`, or `Profinet_Main`.
- A task can be triggered by one or more communication instances and channels.
- A task can read parsed communication input values from its trigger context.
- Any step can write output through one or more already-running communication instances.
- Communication connection lifetime is managed by the system, not by each task.
- Task execution is asynchronous and must have a clear concurrency policy.

## Design Consequences

- Do not model multiple TCP ports or multiple PLC stations as multiple channels under a single protocol object.
- Model each independent endpoint as a `CommunicationInstance`.
- Keep `Channel` as the logical station/work-position inside one communication instance.
- Keep old protocol/channel fields during migration, then map them to default instances.
- Runtime routing should eventually use `CommunicationInstanceName + ChannelName`, not only `CommunicationType`.

## Migration Direction

1. Add communication instance models while keeping existing XML compatible.
2. Add task fields for `CommunicationInstanceName` and trigger bindings while keeping existing protocol/channel fields.
3. Update runtime events to carry `InstanceName`.
4. Update runtime manager from `CommunicationType -> runtime` to `InstanceName -> runtime`.
5. Update task trigger and output routing to use communication instances.
6. Update UI from "communication type" to "communication instance list".
