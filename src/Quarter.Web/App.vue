<template>
    <icons />

    <!-- Wrap the entire application in a div that is first rendered after the current user is initialized. -->
    <div id="q-app" v-if="currentUser.isInitialized" data-testid="initializing">
        <main-navigation></main-navigation>
        <RouterView/>
    </div>
</template>

<script setup lang="ts">
import { RouterView } from "vue-router"
import { useCurrentUser } from "@/stores/use-current-user.ts"
import MainNavigation from "@/components/furniture/MainNavigation.vue"
import Icons from "@/components/furniture/Icons.vue"

const currentUser = useCurrentUser()

// Trigger the initialize, fire and forget
void currentUser.initialize()
</script>
