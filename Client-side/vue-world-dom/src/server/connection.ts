import * as signalR from "@microsoft/signalr";

const url = "http://localhost";
const port = "5000";

export const connection = new signalR.HubConnectionBuilder()
    .withUrl(`${url}:${port}/WorldDominationGame`)
    .withAutomaticReconnect()
    .build();

export const connectionReady = connection.start()
  .catch(err => console.error('LALKA:', err))