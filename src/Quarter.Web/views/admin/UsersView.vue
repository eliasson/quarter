<template>
    <div>
        <h1>Users</h1>
        <div v-for="user in users" :key="user.id">{{ user.email }}</div>
    </div>
</template>

<script setup lang="ts">
import {useUsersStore} from "@/stores/users.ts";

const { users } = useUsersStore()
</script>

<style scoped>

</style>
