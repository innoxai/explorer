import { Observable } from 'babylonjs'

export const ChatEvent = {
  MESSAGE_RECEIVED: 'MESSAGE_RECEIVED',
  MESSAGE_SENT: 'MESSAGE_SENT'
}

export const chatObservable = new Observable()
