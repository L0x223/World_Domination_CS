  <script setup>
  import { Player } from '@/states/player';
  import { ref } from 'vue';
  import { isNickAvailable, addPlayerNickToDb, joinSessionByCode } from '@/server/RequstHandlers';
  import CreateSessionModal from '@/components/CreateSessionModal.vue';
  import { SessionState } from '@/states/session';
  import { LobbyState } from '@/states/lobby';

  let createSessionShowModal = ref(false)
  let sessionNameTaken = ref(false)
  let nickIsTaken = ref(false)

  function handleCreateSession({ sessionId, joinCode, sessionName }) {
    createSessionShowModal.value = false
    processSessionCreation(sessionId, joinCode, sessionName)
    window.location.hash = '/lobby'          
  }


  async function checkNickAvailability(e) {
    
    let nick = e.target.value.trim();
    const available =  await isNickAvailable(nick)

    if (available) {
      nickIsTaken.value = false
      Player.nick = nick
    } else {
      nickIsTaken.value = true
    }
  }

  async function addPlayerNick(e) {
      let isOkay = await addPlayerNickToDb(Player.nick)
      if (isOkay) {
        console.log("Nick added to database successfully")
      } else {
        nickIsTaken.value = true
      }
  }

  async function changeCreateSessionShowModal() {
      await addPlayerNick();
      if (!nickIsTaken.value) {
        createSessionShowModal.value = true
      }
      else {
        console.log("Nick is taken, cannot create modal")
      }
  }

  function processSessionCreation(sessionId, joinCode, sessionName) {
    SessionState.sessionId = sessionId
    SessionState.joinCode = joinCode
    SessionState.sessionName = sessionName
    joinSessionByCode(joinCode, Player.nick)
    console.log("Session created with name: " + sessionName)
    //TODO HANDLE ALL CONSEQUENCES OF SESSION CREATION
  }
</script>

  <template>
    <div>
      <h1 class="title">Welcome to the World Domination Strategy Game</h1>
      
      <div class="form-row">
        <label>your Nick:</label>
        <input @input="checkNickAvailability" placeholder="enter here" class="input" />
      </div>

      <p v-if="nickIsTaken" class="error">This nick is already taken</p>  

      <div class="buttons">
        <button @click="" :disabled="nickIsTaken">Join session</button>
        <button @click="changeCreateSessionShowModal" :disabled="nickIsTaken">Create session</button>
      </div>
    </div>
    <CreateSessionModal :show="createSessionShowModal"
     :nickname="Player.nick"
     v-model:isTaken="sessionNameTaken"
     @close="createSessionShowModal = false" 
     @createSession="handleCreateSession"
     ></CreateSessionModal>
  
  </template>

  <style scoped>
  .title {
    font-size: 2em;
    font-weight: bold;
    text-align: center;
  }
  .form-row {
  display: grid;
  grid-template-columns: 100px 1fr;
  gap: 8px;
  margin-bottom: 8px;
  } 
  .error {
    color: red;
    font-weight: bold;
    font-size: 0.8em;
    margin-left: 108px;
  }

  .input {
    width: 100%;
    max-width: 150px;
  }
  .buttons {
  display: flex;
  gap: 8px;
  } 
  </style>
