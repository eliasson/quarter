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
import { ApiClient } from "@/utils/api-client.ts"

const apiClient = new ApiClient()

const currentUser = useCurrentUser(apiClient)

// Trigger the initialization of the current user, fire and forget
void currentUser.initialize()
</script>
