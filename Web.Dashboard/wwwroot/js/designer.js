export class RemoteDesigner {
    constructor(dotNetRef, container) {
        this.dotNetRef = dotNetRef;
        this.container = container;
        this.isDragging = false;
        this.isResizing = false;
        this.activeElement = null;
        this.startX = 0;
        this.startY = 0;
        this.initialRow = 0;
        this.initialCol = 0;
        this.initialRowSpan = 0;
        this.initialColSpan = 0;
        this.cellWidth = 0;
        this.cellHeight = 0;
        this.pointerId = -1;

        this.onPointerDown = this.onPointerDown.bind(this);
        this.onPointerMove = this.onPointerMove.bind(this);
        this.onPointerUp = this.onPointerUp.bind(this);

        this.container.addEventListener('pointerdown', this.onPointerDown);
        // Attach move/up to window to handle dragging outside container
        window.addEventListener('pointermove', this.onPointerMove);
        window.addEventListener('pointerup', this.onPointerUp);
        window.addEventListener('pointercancel', this.onPointerUp);
    }

    dispose() {
        this.container.removeEventListener('pointerdown', this.onPointerDown);
        window.removeEventListener('pointermove', this.onPointerMove);
        window.removeEventListener('pointerup', this.onPointerUp);
        window.removeEventListener('pointercancel', this.onPointerUp);
    }

    updateGridMetrics() {
        const rect = this.container.getBoundingClientRect();
        // Assuming 5 columns and 12 rows as per CSS
        this.cellWidth = rect.width / 5;
        this.cellHeight = rect.height / 12;
    }

    onPointerDown(e) {
        // Ignore if already interacting
        if (this.isDragging || this.isResizing) return;

        const target = e.target;
        const buttonElement = target.closest('.grid-button');

        if (!buttonElement) return;

        // Check if resizing
        if (target.closest('.resize-handle')) {
            this.isResizing = true;
            // Prepare for resize: make inner absolute so it doesn't affect grid flow
            const inner = buttonElement.querySelector('.grid-button-inner');
            if (inner) {
                const rect = inner.getBoundingClientRect();
                // Lock current size and position
                inner.style.width = `${rect.width}px`;
                inner.style.height = `${rect.height}px`;
                inner.style.position = 'absolute';
                // Use padding from CSS variable or default to 4px
                // Since we are inside a relative parent with padding, 
                // absolute positioning at top/left 0 would ignore padding.
                // We want to respect the padding.
                const computedStyle = window.getComputedStyle(buttonElement);
                const paddingLeft = parseFloat(computedStyle.paddingLeft) || 4;
                const paddingTop = parseFloat(computedStyle.paddingTop) || 4;
                
                inner.style.left = `${paddingLeft}px`;
                inner.style.top = `${paddingTop}px`;
                inner.style.zIndex = '1000';
            }
        } else if (target.closest('.remote-button')) {
            this.isDragging = true;
        } else {
            return;
        }

        this.activeElement = buttonElement;
        this.pointerId = e.pointerId;
        buttonElement.setPointerCapture(e.pointerId);

        this.updateGridMetrics();
        this.startX = e.clientX;
        this.startY = e.clientY;

        // Parse current grid position from style
        // style="grid-row: 2 / span 1; grid-column: 3 / span 2;"
        const style = buttonElement.style;
        const rowMatch = style.gridRow.match(/(\d+)\s*\/\s*span\s*(\d+)/);
        const colMatch = style.gridColumn.match(/(\d+)\s*\/\s*span\s*(\d+)/);

        if (rowMatch && colMatch) {
            // CSS grid is 1-based, our model is 0-based
            this.initialRow = parseInt(rowMatch[1]) - 1;
            this.initialRowSpan = parseInt(rowMatch[2]);
            this.initialCol = parseInt(colMatch[1]) - 1;
            this.initialColSpan = parseInt(colMatch[2]);
        }

        // Add active class for visual feedback
        buttonElement.classList.add('interacting');
        e.preventDefault();
        e.stopPropagation();
    }

    onPointerMove(e) {
        if ((!this.isDragging && !this.isResizing) || !this.activeElement) return;
        if (e.pointerId !== this.pointerId) return;

        e.preventDefault();

        const deltaX = e.clientX - this.startX;
        const deltaY = e.clientY - this.startY;

        if (this.isDragging) {
            this.activeElement.style.transform = `translate(${deltaX}px, ${deltaY}px)`;
            this.activeElement.style.zIndex = '100';
            this.activeElement.style.opacity = '0.8';
        } else if (this.isResizing) {
            // Visual feedback for resizing
            const newWidth = (this.initialColSpan * this.cellWidth) + deltaX;
            const newHeight = (this.initialRowSpan * this.cellHeight) + deltaY;
            
            // Calculate max available space based on grid boundaries
            // Grid is 5 cols x 12 rows
            const maxColsAvailable = 5 - this.initialCol;
            const maxRowsAvailable = 12 - this.initialRow;
            
            const maxWidth = maxColsAvailable * this.cellWidth;
            const maxHeight = maxRowsAvailable * this.cellHeight;

            // Constrain size (min 0.5 cell, max available space)
            const constrainedWidth = Math.min(Math.max(this.cellWidth * 0.5, newWidth), maxWidth);
            const constrainedHeight = Math.min(Math.max(this.cellHeight * 0.5, newHeight), maxHeight);

            const inner = this.activeElement.querySelector('.grid-button-inner');
            if (inner) {
                inner.style.width = `${constrainedWidth}px`;
                inner.style.height = `${constrainedHeight}px`;
                this.activeElement.style.zIndex = '100';
            }
        }
    }

    onPointerUp(e) {
        if ((!this.isDragging && !this.isResizing) || !this.activeElement) return;
        if (e.pointerId !== this.pointerId) return;

        const deltaX = e.clientX - this.startX;
        const deltaY = e.clientY - this.startY;

        // Calculate grid deltas
        const colDelta = Math.round(deltaX / this.cellWidth);
        const rowDelta = Math.round(deltaY / this.cellHeight);

        let newRow = this.initialRow;
        let newCol = this.initialCol;
        let newRowSpan = this.initialRowSpan;
        let newColSpan = this.initialColSpan;

        if (this.isDragging) {
            newRow = this.initialRow + rowDelta;
            newCol = this.initialCol + colDelta;
        } else if (this.isResizing) {
            newRowSpan = Math.max(1, this.initialRowSpan + rowDelta);
            newColSpan = Math.max(1, this.initialColSpan + colDelta);
        }

        // Boundary checks (0-based index, max 5 cols, 12 rows)
        // Clamp position
        newRow = Math.max(0, Math.min(newRow, 12 - newRowSpan));
        newCol = Math.max(0, Math.min(newCol, 5 - newColSpan));
        
        // Clamp size
        newRowSpan = Math.min(newRowSpan, 12 - newRow);
        newColSpan = Math.min(newColSpan, 5 - newCol);

        // Reset styles
        this.activeElement.style.transform = '';
        this.activeElement.style.zIndex = '';
        this.activeElement.style.opacity = '';
        this.activeElement.classList.remove('interacting');
        
        const inner = this.activeElement.querySelector('.grid-button-inner');
        if (inner) {
            inner.style.width = '';
            inner.style.height = '';
            inner.style.position = '';
            inner.style.top = '';
            inner.style.left = '';
            inner.style.zIndex = '';
        }

        // Get ID from data attribute
        const buttonId = this.activeElement.dataset.id;

        // Invoke Blazor
        if (buttonId) {
            this.dotNetRef.invokeMethodAsync('UpdateButtonLayout', buttonId, newRow, newCol, newRowSpan, newColSpan);
        }

        this.isDragging = false;
        this.isResizing = false;
        this.activeElement = null;
        this.pointerId = -1;
    }
}

export function initDesigner(dotNetRef) {
    const container = document.querySelector('.designer-grid');
    if (container) {
        return new RemoteDesigner(dotNetRef, container);
    }
    return null;
}
