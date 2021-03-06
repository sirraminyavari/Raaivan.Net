/*!
 * jquery.dad.js v2 (http://konsolestudio.com/dad)
 * Author William Lima
 */

(function ($) {
    "use strict";

    var global = {};

    global.supportsTouch = "ontouchstart" in window || navigator.msMaxTouchPoints;
    global.shouldScroll = true;

    if (global.supportsTouch) {
        var scrollListener = function (e) {
            if (!global.shouldScroll) {
                e.preventDefault();
            }
        };

        document.addEventListener("touchmove", scrollListener, { passive: false });
    }

    /**
     * Mouse constructor
     */
    function DadMouse() {
        this.positionX = 0;
        this.positionY = 0;
        this.offsetX = 0;
        this.offsetY = 0;
    }

    /**
     * Mouse udpate event
     * @param {Event}
     */
    DadMouse.prototype.update = function (e) {
        // Check if it is touch
        if (global.supportsTouch && e.type == "touchmove") {
            var targetEvent = e.originalEvent.touches[0];
            var mouseTarget = document.elementFromPoint(
                targetEvent.clientX,
                targetEvent.clientY
            );
            $(mouseTarget).trigger("touchenter");

            // update mouse coordinates from touch
            this.positionX = targetEvent.pageX;
            this.positionY = targetEvent.pageY;
        } else {
            this.positionX = e.pageX;
            this.positionY = e.pageY;
        }
    };

    /**
     * DAD class constructor
     * @param {element} element
     * @param {options} options
     */
    function Dad(element, options) {
        this.options = this.parseOptions(options);

        // jQuery elements
        this.$container = $(element);
        this.$current = null;
        this.$target = null;
        this.$clone = null;

        // Inner variables
        this.mouse = new DadMouse();
        this.holding = false;
        this.dragging = false;
        this.dropzones = [];

        // Configure and setup
        this.setActive(this.options.active);
        this.setup();
    }

    /**
     * Static attribute that stores default dad options
     */
    Dad.defaultOptions = {
        active: true,
        draggable: false,
        exchangeable: true,
        transition: 200,
        placeholderTarget: false,
        placeholderTemplate: "<div />",
        targetFilters: [] // { 'draggable': '', 'droppable': '' }
    };

    /**
     * Merge provided options with the defaults
     */
    Dad.prototype.parseOptions = function (options) {
        // Make defaults immutable
        var parsedOptions = $.extend(true, {}, Dad.defaultOptions);

        if (options) {
            $.each(parsedOptions, function (key, value) {
                var overrideValue = options[key];

                if (typeof overrideValue !== "undefined") {
                    // Valid for arrays as well
                    if (typeof overrideValue === "object") {
                        parsedOptions[key] = $.extend(parsedOptions[key], overrideValue);
                    } else {
                        parsedOptions[key] = overrideValue;
                    }
                }
            });
        }

        return parsedOptions;
    };

    Dad.prototype.validateDrop = function (dragItem, dropZone) {
        var filters = (this.options.targetFilters || []).filter(op => !!op && !!op.draggable && !!op.droppable);
        return !!dragItem && !!dropZone &&
            (!filters.length || filters.some(f => $(dragItem).hasClass(f.draggable) && $(dropZone).hasClass(f.droppable)));
    };

    /**
     * Add all required listeners and
     * styles that prevents some issues when dragging
     */
    Dad.prototype.setup = function () {
        var self = this;

        // Prevent user from highlight text
        this.$container.css({
            position: "relative",
            "-webkit-touch-callout": "none",
            "-webkit-user-select": "none",
            "-khtml-user-select": "none",
            "-moz-user-select": "none",
            "-ms-user-select": "none",
            "user-select": "none",
        });

        // Prevent dragging images on IE
        this.$container.find("img").attr("ondragstart", "return false");

        // Create a callback for click event
        function onChildClick(e) {
            var $target = $(this);
            self.prepare(e, $target);
        }

        // Create a callback for enter event
        function onChildEnter(e) {
            if (self.$current) {
                var $this = $(this);
                var isFromCurrent = !!self.$current.find(this).length;
                var isExchangeable = self.options.exchangeable;

                var shouldExchange = self.dragging && (isFromCurrent || isExchangeable) &&
                    self.validateDrop($this.get(0), $this.get(0).parentNode);
                
                if (shouldExchange) {
                    self.updatePlaceholder(e, $this);
                }
            }
        }

        // Set container comunication
        this.$container.on("mouseenter touchenter", function (e) {
            if (self.$current) {
                var $this = $(this);
                var isNotCurrent = !$this.is(self.$current);
                var isExchangeable = self.options.exchangeable;

                var shouldExchange = self.dragging && isNotCurrent && isExchangeable &&
                    self.validateDrop(self.$target, $this);
                
                if (shouldExchange) {
                    self.$current = $this;
                    self.updatePlaceholder(e, $this, true);
                }
            }
        });

        // Add element event listeners
        this.$container.on("mousedown touchstart", "> *", onChildClick);
        this.$container.on("mouseenter touchenter", "> *", onChildEnter);

        // Add window event listeners
        $("body").on("mousemove touchmove", this.update.bind(this));
        $("body").on("mouseup touchend", this.end.bind(this));

        // Cancelling drag due to browser native actions
        // Note: Using window on mouseleave causes a bug...
        $("body").on("mouseleave", this.end.bind(this));
        $(window).on("blur", this.end.bind(this));
    };

    /**
     * Prepare container to start dragging
     *
     * @param {*} event click/mousedown event
     * @param {*} element target element
     */
    Dad.prototype.prepare = function (e, $target) {
        var draggable = this.options.draggable;
        var $draggable = draggable && $(draggable);
        var shouldStartDragging =
            this.active &&
            ($draggable
                ? $draggable.is(e.target) || $draggable.find(e.target).length
                : true);

        if (shouldStartDragging) {
            this.holding = true;
            this.$target = $target;
            this.$current = $target.closest(this.$container);
            this.mouse.update(e);
        }
    };

    /**
     * First step, occurs on mousedown
     * @param {Event}
     */
    Dad.prototype.start = function (e) {
        // Set target and get its metrics
        var $target = this.$target;

        // Add clone
        var $clone = $target.clone().css({
            position: "fixed", //"absolute", --> ramin
            zIndex: 9999999,
            pointerEvents: "none",
            height: $target.outerHeight(),
            width: $target.outerWidth(),
        });

        // Add placeholder
        var $placeholder = $(this.options.placeholderTemplate).css({
            position: "absolute",
            pointerEvents: "none",
            zIndex: 9999998,
            margin: 0,
            padding: 0,
            height: $target.outerHeight(),
            width: $target.outerWidth(),
        });

        // Set mouse offset values
        this.mouse.offsetX = this.mouse.positionX - $target.offset().left;
        this.mouse.offsetY = this.mouse.positionY - $target.offset().top;

        $target.css("visibility", "hidden");
        $target.attr("data-dad-target", true);

        // Setting variables
        if (global.supportsTouch) global.shouldScroll = false;

        this.dragging = true;
        this.$target = $target;
        this.$clone = $clone;
        this.$placeholder = $placeholder;

        // Add elements to container
        this.$current.append($placeholder).append($clone);

        // Set clone and placeholder position
        this.updateClonePosition();
        this.updatePlaceholderPosition();
    };

    /**
     * Middle step, occurs on mousemove
     */
    Dad.prototype.update = function (e) {
        this.mouse.update(e);

        // If user is holding but not dragging
        // Call start method
        if (this.holding && !this.dragging) {
            this.start(e);
        }

        if (this.dragging) {
            this.updateClonePosition();
        }
    };

    /**
     * Final step, ocurrs on mouseup
     */
    Dad.prototype.end = function (e) {
        this.holding = false;

        // Finish dragging if is dragging
        if (this.dragging) {
            if (global.supportsTouch) global.shouldScroll = true;

            var $current = this.$current;
            var $target = this.$target;
            var $clone = this.$clone;
            var $placeholder = this.$placeholder;

            var animateToX = $target.offset().left;// - $current.offset().left; --> ramin
            var animateToY = $target.offset().top;// - $current.offset().top; --> ramin

            // Trigger callback
            $($current).trigger("dadDropStart", [$target[0]]);

            // Do transition from clone to target
            $clone.animate(
                {
                    top: animateToY,
                    left: animateToX,
                    height: $target.outerHeight(),
                    width: $target.outerWidth(),
                },
                this.options.transition,
                function () {
                    // Remove dad elements
                    $clone.remove();
                    $placeholder.remove();

                    // Normalize target
                    $target.removeAttr("data-dad-target");
                    $target.css("visibility", "");

                    // On dad dropped
                    $($current).trigger("dadDropEnd", [$target[0]]);
                }
            );

            // Reset variables
            this.dragging = false;

            // Reset elements
            this.$current = null;
            this.$target = null;
            this.$clone = null;
            this.$placeholder = null;
        }
    };

    /**
     * Dad update clone position based on the mouse position
     */
    Dad.prototype.updateClonePosition = function () {
        // Get positions

        var targetX =
            this.mouse.positionY /*- this.$current.offset().top*/ - this.mouse.offsetY; //--> ramin
        var targetY =
            this.mouse.positionX /*- this.$current.offset().left*/ - this.mouse.offsetX; //--> ramin

        // Update clone
        this.$clone.css({ top: targetX, left: targetY });
    };

    /**
     * Dad update placeholder position by
     * checking the current placeholder position
     */
    Dad.prototype.updatePlaceholder = function (e, $element, isContainer) {
        if (isContainer) {
            // Move target
            $element.append(this.$target);
            
            // And also move dad elements for positioning
            $element.append(this.$clone);
            $element.append(this.$placeholder);
        } else {
            if ($element.index() > this.$target.index()) {
                $element.after(this.$target);
            } else {
                $element.before(this.$target);
            }
        }

        this.updatePlaceholderPosition();
    };

    /**
     * Update placeholder position based on its options
     */
    Dad.prototype.updatePlaceholderPosition = function () {
        var placeholderTarget = this.options.placeholderTarget;

        var $target = placeholderTarget
            ? this.$target.find(placeholderTarget)
            : this.$target;

        var targetTop = $target.offset().top - this.$current.offset().top;
        var targetLeft = $target.offset().left - this.$current.offset().left;
        var targetHeight = $target.outerHeight();
        var targetWidth = $target.outerWidth();

        this.$placeholder.css({
            top: targetTop,
            left: targetLeft,
            width: targetWidth,
            height: targetHeight,
        });
    };

    Dad.prototype.onDrop = function (selector, onDrop) {
        var $dropzone = $(selector);

        function onDropzoneEnter(e) {
            var $this = $(this);

            $this.attr("data-dad-active", true);
        }

        $dropzone.on("mouseenter touchenter", onDropzoneEnter);
    };

    /**
     * Update container active status which later
     * will prevent the dragging to start on the prepare function
     */
    Dad.prototype.setActive = function (isActive) {
        this.active = isActive;
        this.$container.attr("data-dad-active", isActive);
    };

    Dad.prototype.activate = function () {
        this.setActive(true);
    };

    Dad.prototype.deactivate = function () {
        this.setActive(false);
    };

    $.fn.dad = function (options) {
        return new Dad(this, options);
    };
})(jQuery);