  <script setup>
  import { Player } from '@/states/player';
  import { ref } from 'vue';
  import { isNickAvailable, addPlayerNickToDb, joinSessionByCode, joinSessionById } from '@/server/RequstHandlers';
  import CreateSessionModal from '@/components/CreateSessionModal.vue';
  import { SessionState } from '@/states/session';
  import { LobbyState } from '@/states/lobby';
  import SearchSessionModal from '@/components/SearchSessionModal.vue';

  const createSessionShowModal = ref(false)
  const searchSessionShowModal = ref(false)
  const sessionNameTaken = ref(false)
  const nickIsTaken = ref(false)
  const localNick = ref(Player.nick)

  function handleCreateSession({ sessionId, joinCode, sessionName }) {
    createSessionShowModal.value = false
    processSessionCreation(sessionId, joinCode, sessionName)
    console.log("Session created with id: " + sessionId)
    window.location.hash = '/lobby'          
  }

  function handleJoinSession(sessionId) {
    searchSessionShowModal.value = false
    processSessionJoin(sessionId)
    SessionState.justJoined = true
    window.location.hash = '/lobby'
  }
  async function checkNickAvailability() {
    const nick = localNick.value.trim()
    Player.nick = nick

    if (nick === '') {
      nickIsTaken.value = false
      return
    }

    const available = await isNickAvailable(nick)
    if (localNick.value.trim() !== nick) return
    nickIsTaken.value = !available
  }

  async function addPlayerNick(e) {
  console.log("Attempting to add nick:", JSON.stringify(Player.nick))
  const result = await addPlayerNickToDb(Player.nick)
  console.log("Result from addPlayerNickToDb:", result)
  if (result.success) {
    Player.id = result.playerId
    console.log("Nick added to database successfully, id:", Player.id)
  } else {
    nickIsTaken.value = true
  }
}

async function ensureNickRegistered() {
  const nick = Player.nick

  if (!nick) {
    nickIsTaken.value = true
    return false
  }

  if (Player.id && Player.registeredNick === nick) {
    nickIsTaken.value = false
    return true
  }

  const result = await addPlayerNickToDb(nick)
  if (result.success) {
    Player.id = result.playerId
    Player.registeredNick = nick
    nickIsTaken.value = false
    return true
  } else {
    nickIsTaken.value = true
    return false
  }
}
async function changeCreateSessionShowModal() {
  if (await ensureNickRegistered()) {
    createSessionShowModal.value = true
  } else {
    console.log("Nick is taken, cannot create modal")
  }
}

async function changeSearchSessionShowModal() {
  if (await ensureNickRegistered()) {
    searchSessionShowModal.value = true
  } else {
    console.log("Nick is taken, cannot search session")
  }
}

  async function processSessionCreation(sessionId, joinCode, sessionName) {
    SessionState.sessionId = sessionId
    SessionState.joinCode = joinCode
    SessionState.sessionName = sessionName

    const result = await joinSessionByCode(joinCode, Player.nick)
    if (result.success) {
      SessionState.justJoined = true
    } else {
      console.error('Failed to join own session:', result.reason)
    }
  }
  
  async function processSessionJoin(sessionId) {
    SessionState.sessionId = sessionId

    const result = await joinSessionById(sessionId, Player.nick)
    if (result.success) {
      SessionState.justJoined = true
    } else {
      console.error('Failed to join session:', result.reason)
    }
  }
</script>

  <template>
    <div>
      <h1 class="title">Welcome to the World Domination Strategy Game</h1>
      
      <div class="form-row">
        <label>your Nick:</label>
        <input v-model="localNick" @input="checkNickAvailability" placeholder="enter here" class="input" />
      </div>

      <p v-if="nickIsTaken" class="error">This nick is already taken</p>  

      <div class="buttons">
        <button @click="changeSearchSessionShowModal" :disabled="nickIsTaken">Search session</button>
        <button @click="changeCreateSessionShowModal" :disabled="nickIsTaken">Create session</button>
      </div>
    </div>
    <CreateSessionModal :show="createSessionShowModal"
     :nickname="Player.nick"
     v-model:isTaken="sessionNameTaken"
     @close="createSessionShowModal = false" 
     @createSession="handleCreateSession"
     ></CreateSessionModal>
    <SearchSessionModal :show="searchSessionShowModal"
    @joinSession="handleJoinSession"
    @close="searchSessionShowModal = false"
     />
  
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
