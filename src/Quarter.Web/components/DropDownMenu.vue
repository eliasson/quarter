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
            <template v-for="item in items">
                <RouterLink :to="item.link" class="q-menu-item" data-testid="menu-item">
                    <div class="q-menu-item--icon" data-testid="menu-icon">
                        <svg class="q-icon">
                            <use :xlink:href="item.iconHref()"></use>
                        </svg>
                    </div>
                    <div class="q-menu-item--content">
                        <div class="q-menu-item--title" data-testid="menu-title">{{ item.title }}</div>
                        <div class="q-menu-item--sub-title" data-testid="menu-sub-title">{{ item.subTitle }}</div>
                    </div>
                </RouterLink>
            </template>
        </div>
    </div>
</template>

<script setup lang="ts">
import { MenuItem } from "@/models/ui.ts"
import { computed, ref } from "vue"
import { RouterLink } from "vue-router"

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
