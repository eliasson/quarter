<template>
    <div data-testid="drop-down-menu">
        <slot name="trigger" :trigger="toggle">
            <button type="button"
                    class="q-button q-button__round"
                    :title="triggerTitle"
                    @click="toggle"
                    data-testid="trigger">
                <svg class="q-icon">
                    <use :xlink:href="iconHref"></use>
                </svg>
            </button>
        </slot>
        <div v-if="isVisible" class="q-drop-down-menu" data-testid="drop-down-menu-modal">
            <!-- Render menu items -->
        </div>
    </div>
</template>

<script setup lang="ts">
import { MenuItem } from "@/models/ui.ts"
import {computed, ref} from "vue"

const props = defineProps<{
    triggerIcon: string
    triggerTitle: string
    items: Array<MenuItem>
}>()

const isVisible = ref(false)

const toggle = () => isVisible.value = !isVisible.value
const iconHref = computed(() => `#${props.triggerIcon}`)
</script>

<style scoped>

</style>
