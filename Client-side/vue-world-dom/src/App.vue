  <script setup lang="ts">
  import * as signalR from "@microsoft/signalr";
  import { connection } from "./server/connection";
  import { currentView } from "./router";
  import { registerHandlers } from "./server/MessageHandler";


  registerHandlers();

  connection
      .start()
      .then(() => console.log("Connected to SignalR hub"))
      .catch((err) => {
        console.error("SignalR connection error:", err);
      });
  </script>
<template>
  <component :is="currentView" />
</template>

  <style scoped></style>
