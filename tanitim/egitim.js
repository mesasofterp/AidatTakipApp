// Training Page JavaScript

// Sidebar active link on scroll
const sections = document.querySelectorAll('.module-section');
const sidebarLinks = document.querySelectorAll('.sidebar-link');

window.addEventListener('scroll', () => {
    let current = '';
    
    sections.forEach(section => {
        const sectionTop = section.offsetTop;
        const sectionHeight = section.clientHeight;
        
        if (window.pageYOffset >= sectionTop - 150) {
            current = section.getAttribute('id');
        }
    });

    sidebarLinks.forEach(link => {
        link.classList.remove('active');
        if (link.getAttribute('href') === `#${current}`) {
            link.classList.add('active');
        }
    });
});

// Smooth scroll for sidebar links
sidebarLinks.forEach(link => {
    link.addEventListener('click', (e) => {
        e.preventDefault();
        const targetId = link.getAttribute('href');
        const targetSection = document.querySelector(targetId);
        
        if (targetSection) {
            const offsetTop = targetSection.offsetTop - 100;
            window.scrollTo({
                top: offsetTop,
                behavior: 'smooth'
            });
        }
    });
});

// Animate sections on scroll
const observerOptions = {
    threshold: 0.1,
    rootMargin: '0px 0px -100px 0px'
};

const contentObserver = new IntersectionObserver((entries) => {
    entries.forEach(entry => {
        if (entry.isIntersecting) {
            entry.target.style.opacity = '1';
            entry.target.style.transform = 'translateY(0)';
        }
    });
}, observerOptions);

// Observe content blocks
document.querySelectorAll('.content-block').forEach(block => {
    block.style.opacity = '0';
    block.style.transform = 'translateY(30px)';
    block.style.transition = 'opacity 0.6s ease, transform 0.6s ease';
    contentObserver.observe(block);
});

// Add stagger effect
document.querySelectorAll('.content-block').forEach((block, index) => {
    block.style.transitionDelay = `${index * 0.1}s`;
});

// Image lazy loading fallback
document.querySelectorAll('.image-container img').forEach(img => {
    img.addEventListener('error', function() {
        this.style.display = 'none';
        const container = this.parentElement;
        if (container) {
            container.innerHTML = `
                <div style="padding: 3rem; text-align: center; background: var(--bg-light);">
                    <i class="fas fa-image" style="font-size: 3rem; color: var(--text-light); margin-bottom: 1rem;"></i>
                    <p style="color: var(--text-light);">Görsel yüklenemedi</p>
                </div>
            `;
        }
    });
});

// Video container responsive handling
document.querySelectorAll('.video-container iframe').forEach(iframe => {
    iframe.addEventListener('load', function() {
        // Video loaded successfully
        console.log('Video loaded');
    });
});

// Mobile sidebar toggle
const sidebarToggle = document.createElement('button');
sidebarToggle.className = 'sidebar-toggle';
sidebarToggle.innerHTML = '<i class="fas fa-bars"></i>';
sidebarToggle.style.cssText = `
    display: none;
    position: fixed;
    bottom: 20px;
    right: 20px;
    width: 50px;
    height: 50px;
    border-radius: 50%;
    background: var(--primary-color);
    color: white;
    border: none;
    box-shadow: var(--shadow-lg);
    z-index: 999;
    cursor: pointer;
    font-size: 1.2rem;
`;

if (window.innerWidth <= 1024) {
    document.body.appendChild(sidebarToggle);
    
    sidebarToggle.addEventListener('click', () => {
        const sidebar = document.querySelector('.training-sidebar');
        sidebar.classList.toggle('mobile-open');
    });
}

window.addEventListener('resize', () => {
    if (window.innerWidth <= 1024) {
        if (!document.body.contains(sidebarToggle)) {
            document.body.appendChild(sidebarToggle);
        }
    } else {
        if (document.body.contains(sidebarToggle)) {
            sidebarToggle.remove();
        }
    }
});

// Add mobile sidebar styles
const mobileStyle = document.createElement('style');
mobileStyle.textContent = `
    @media (max-width: 1024px) {
        .training-sidebar {
            position: fixed;
            top: 70px;
            left: -100%;
            width: 300px;
            height: calc(100vh - 70px);
            z-index: 998;
            transition: left 0.3s ease;
            overflow-y: auto;
        }
        
        .training-sidebar.mobile-open {
            left: 0;
        }
        
        .sidebar-sticky {
            position: relative;
            top: 0;
            height: 100%;
        }
    }
`;
document.head.appendChild(mobileStyle);

