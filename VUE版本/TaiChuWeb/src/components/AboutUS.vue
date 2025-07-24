<template>
    <div class="container">
        <div class="container-wrapper">
        <div class="logo">
            <div class="logo-inner">
                <i class="fas fa-sun"></i>
            </div>
        </div>
        
        <div class="section">
            <h1>自2015年出发，于2025正式成立</h1>
            <p class="text_secondary">一段从激情到坚持的创业旅程</p>
        </div>
        
        <div class="section">
            <h1>我们以柴圈为起点，立志打造独特的IP生态圈</h1>
            <p class="text_secondary">我们正在努力整合柴圈资源，促进柴友间的深度互动与交流</p>
            <div class="mission-statement">
                从最初的灵感到今日的实践，我们始终坚守初心：<span class="highlight">打造最具创意的文化生态系统</span>
            </div>
        </div>
        
        <div class="section">
            <h1>我们是八九点钟的太阳</h1>
            <p class="text_secondary">以传播中华文化为使命，发扬中华思想的精髓</p>
            <p class="text_secondary">致力于通过作品让世界变得更美好、更温暖</p>
            <p class="text_secondary"><span class="highlight">光荣在于平淡，艰巨在于漫长</span></p>
        </div>
        
        <div class="section">
            <h1>关注我们</h1>
            <div class="link">
                <a href="https://space.bilibili.com/3546736970696727" target="_blank">
                    <i class="fab fa-bilibili"></i> B站官号：太初寰宇
                </a>
                <p class="feedback-text">如果你们任何想法或者建议，很欢迎你们进入我们的反馈群进行提供反馈！（下方QQ群）</p>
            </div>
            
            <div class="social-icons">
                <a href="https://qm.qq.com/q/DQ6fkPgd2w" target="_blank"><i class="fab fa-weixin"></i>QQ</a>
                <a href="#"><i class="fab fa-weibo"></i>暂无</a>
                <a href="#"><i class="fab fa-youtube"></i>暂无</a>
                <a href="#"><i class="fab fa-tiktok"></i>暂无</a>
            </div>
        </div>
        
        <div class="copyright">
            &copy; 2025 太初寰宇 - 让世界因我们而更美好
        </div>
        </div>
    </div>
</template>

<script setup>



// 添加滚入动画效果
        document.addEventListener('DOMContentLoaded', function() {
            const sections = document.querySelectorAll('.section');
            
            const observer = new IntersectionObserver((entries) => {
                entries.forEach(entry => {
                    if (entry.isIntersecting) {
                        entry.target.style.transform = 'translateY(0)';
                        entry.target.style.opacity = '1';
                    }
                });
            }, { threshold: 0.1 });
            
            sections.forEach(section => {
                observer.observe(section);
            });
        });
</script>

<style scoped>

@import url('/static/css/AboutUS.css');
    
@import url('/static/css/general.css');



</style>