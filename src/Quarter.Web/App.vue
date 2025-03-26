<template>
    <!-- Quarter is a tiny application, it is made to fit in a fairly small, centered view on the page. -->
    <div id="q-app">
        <nav>
            <RouterLink to="/">Home</RouterLink>
            <RouterLink to="/about">About</RouterLink>
        </nav>
        <main>
            <RouterView/>
        </main>
    </div>
</template>

<script setup lang="ts">
import { RouterLink, RouterView } from 'vue-router'
</script>
