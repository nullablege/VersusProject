// ============================================
// KIYASLASANA - INTERACTIONS
// ============================================

document.addEventListener('DOMContentLoaded', function() {
  // Initialize all components
  initTabs();
  initMobileMenu();
  initCompareSlots();
  initFilterChips();
  initSmoothScroll();
});

// ============================================
// TABS
// ============================================
function initTabs() {
  const tabContainers = document.querySelectorAll('.tabs');
  
  tabContainers.forEach(container => {
    const tabs = container.querySelectorAll('.tab');
    const tabPanels = document.querySelectorAll('.tab-panel');
    
    tabs.forEach(tab => {
      tab.addEventListener('click', () => {
        const targetId = tab.dataset.tab;
        
        // Update active tab
        tabs.forEach(t => t.classList.remove('active'));
        tab.classList.add('active');
        
        // Update active panel
        tabPanels.forEach(panel => {
          panel.classList.add('hidden');
          if (panel.id === targetId) {
            panel.classList.remove('hidden');
          }
        });
      });
    });
  });
}

// ============================================
// MOBILE MENU
// ============================================
function initMobileMenu() {
  const menuBtn = document.querySelector('.mobile-menu-btn');
  const mobileNav = document.querySelector('.mobile-nav');
  
  if (menuBtn && mobileNav) {
    menuBtn.addEventListener('click', () => {
      mobileNav.classList.toggle('hidden');
      const isExpanded = !mobileNav.classList.contains('hidden');
      menuBtn.setAttribute('aria-expanded', isExpanded);
    });
  }
}

// ============================================
// COMPARE SLOTS
// ============================================
function initCompareSlots() {
  const removeBtns = document.querySelectorAll('.compare-slot-remove');
  
  removeBtns.forEach(btn => {
    btn.addEventListener('click', (e) => {
      e.stopPropagation();
      const slot = btn.closest('.compare-slot');
      
      // Animate removal
      slot.style.transform = 'scale(0.9)';
      slot.style.opacity = '0';
      
      setTimeout(() => {
        slot.classList.remove('filled');
        slot.innerHTML = `
          <div class="compare-slot-empty">
            <svg width="32" height="32" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
              <rect x="3" y="3" width="18" height="18" rx="2"/>
              <line x1="12" y1="8" x2="12" y2="16"/>
              <line x1="8" y1="12" x2="16" y2="12"/>
            </svg>
            <p>Telefon ekle</p>
          </div>
        `;
        slot.style.transform = '';
        slot.style.opacity = '';
      }, 200);
    });
  });
}

// ============================================
// FILTER CHIPS
// ============================================
function initFilterChips() {
  const filterChips = document.querySelectorAll('.filter-chip');
  
  filterChips.forEach(chip => {
    chip.addEventListener('click', () => {
      chip.classList.toggle('active');
    });
  });
}

// ============================================
// SMOOTH SCROLL
// ============================================
function initSmoothScroll() {
  document.querySelectorAll('a[href^="#"]').forEach(anchor => {
    anchor.addEventListener('click', function(e) {
      e.preventDefault();
      const target = document.querySelector(this.getAttribute('href'));
      if (target) {
        target.scrollIntoView({
          behavior: 'smooth',
          block: 'start'
        });
      }
    });
  });
}

// ============================================
// SCROLL TO TOP BUTTON
// ============================================
function initScrollToTop() {
  const scrollBtn = document.querySelector('.scroll-to-top');
  
  if (scrollBtn) {
    window.addEventListener('scroll', () => {
      if (window.scrollY > 500) {
        scrollBtn.classList.remove('hidden');
      } else {
        scrollBtn.classList.add('hidden');
      }
    });
    
    scrollBtn.addEventListener('click', () => {
      window.scrollTo({
        top: 0,
        behavior: 'smooth'
      });
    });
  }
}

// ============================================
// PRODUCT CARD HOVER EFFECTS
// ============================================
document.querySelectorAll('.product-card').forEach(card => {
  card.addEventListener('mouseenter', function() {
    this.style.transform = 'translateY(-4px)';
  });
  
  card.addEventListener('mouseleave', function() {
    this.style.transform = '';
  });
});

// ============================================
// SEARCH INPUT FOCUS
// ============================================
const searchInputs = document.querySelectorAll('.search-input, .hero-search-input');
searchInputs.forEach(input => {
  input.addEventListener('focus', function() {
    this.parentElement.classList.add('focused');
  });
  
  input.addEventListener('blur', function() {
    this.parentElement.classList.remove('focused');
  });
});

// ============================================
// ACCESSIBILITY - Skip to content
// ============================================
document.addEventListener('keydown', function(e) {
  if (e.key === 'Tab' && e.shiftKey && document.activeElement === document.body) {
    const skipLink = document.querySelector('.skip-to-content');
    if (skipLink) {
      e.preventDefault();
      skipLink.focus();
    }
  }
});