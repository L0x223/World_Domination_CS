<script setup lang="ts">
import { currentView } from "./router";
import { registerHandlers } from "./server/MessageHandler";
import { connectionReady } from "./server/connection";

registerHandlers();

connectionReady
  .then(() => console.log("Connected to SignalR hub"));
</script>

<template>
  <component :is="currentView" />
</template>

<style scoped></style>