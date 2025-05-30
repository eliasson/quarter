<template>
    <div data-testid="drop-down-menu">
        <slot name="trigger"></slot>
        <div v-if="isVisible" class="q-drop-down-menu" data-testid="drop-down-menu-modal"></div>
    </div>
</template>

<script setup lang="ts">
import { MenuItem } from "@/models/ui.ts"
import { ref } from "vue"

defineProps<{
    items: Array<MenuItem>
}>()

const isVisible = ref(false)
</script>

<style scoped>

</style>
