import { defineStore } from 'pinia';

export const usePublisherStore = defineStore('publisher', {
  state: () => ({
    isOpen: false,
  }),
  actions: {
    open() { this.isOpen = true; },
    close() { this.isOpen = false; },
    toggle() { this.isOpen = !this.isOpen; }
  }
});