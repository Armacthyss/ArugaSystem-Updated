<template>
  <div style="background: #fff8ec; min-height: 100vh; padding: 50px; text-align: center;">
    <h1 style="color: #2d3a26; font-size: 3rem;">Welcome to Page A</h1>
    <p style="color: #5d6b52; margin-bottom: 20px;">This is your root login page.</p>
    
    <button 
      @click="$router.push('/page-b')" 
      style="padding: 15px 30px; background: #546b41; color: white; border: none; border-radius: 12px; cursor: pointer; font-weight: bold;"
    >
      Go to Staff Login (Page B) →
    </button>
     <button 
      @click="$router.push('/Login')" 
      style="padding: 15px 30px; background: #546b41; color: white; border: none; border-radius: 12px; cursor: pointer; font-weight: bold;"
    >
     Parent Login
    </button>
  </div>
</template>

<script setup>
// Navigation logic is handled by $router in the template
</script>