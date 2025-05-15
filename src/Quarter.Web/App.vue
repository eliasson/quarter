<template>
    <!-- Wrap the entire application in a div that is first rendered after the current user is initialized. -->
    <div v-if="currentUser.isInitialized" data-testid="initializing" style="background-color: darkred">
        <!-- Quarter is a tiny application, it is made to fit in a fairly small, centered view on the page. -->
        <div id="q-app">
            <nav>
                <RouterLink to="/">Home</RouterLink>
                <RouterLink to="/about">About</RouterLink>
                <RouterLink to="/admin/users">Users</RouterLink>
            </nav>
            <main>
                <RouterView/>
            </main>
        </div>
    </div>
</template>

<script setup lang="ts">
import { RouterLink, RouterView } from "vue-router"
import { useCurrentUser } from "@/stores/use-current-user.ts"

const currentUser = useCurrentUser()

// Trigger the initialize, fire and forget
void currentUser.initialize()

</script>
