import { reactive, watch } from 'vue'

const STORAGE_KEY = 'wd-session-state'

function loadInitial() {
  try {
    const raw = sessionStorage.getItem(STORAGE_KEY)
    return raw ? JSON.parse(raw) : { sessionId: null, joinCode: null, sessionName: null, justJoined: false }
  } catch {
    return { sessionId: null, joinCode: null, sessionName: null, justJoined: false }
  }
}

export const SessionState = reactive(loadInitial())

watch(SessionState, (value) => {
  sessionStorage.setItem(STORAGE_KEY, JSON.stringify(value))
}, { deep: true })