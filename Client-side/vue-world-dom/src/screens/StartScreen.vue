  <script setup>
  import { Player } from '@/states/player';
  import { ref } from 'vue';
  import { isNickAvailable, addPlayerNickToDb } from '@/server/RequstHandlers';
  import CreateSessionModal from '@/components/CreateSessionModal.vue';

  let createSessionShowModal = ref(false)
  let nickIsTaken = ref(false)

  function handleCreateSession(sessionName) {
  createSessionShowModal.value = false

  ws.createSession(sessionName)

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
        <button @click="">Join session</button>
        <button @click="createSessionShowModal = true">Create session</button>
      </div>
    </div>
    <CreateSessionModal :show="createSessionShowModal"
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
