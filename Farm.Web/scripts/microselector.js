
    var templateCache = [];
    function foreach(fn, nodes) {
        for (var i = nodes.length - 1; i >= 0; i -= 1) {
            fn(nodes[i]);
        }
    }
    
     function callAjax(url, callback, async) {
        if (url == undefined)
            return;
        async = async === undefined ? true : async;
        var xmlhttp;
        xmlhttp = new XMLHttpRequest();
        xmlhttp.onreadystatechange = function () {
            if (xmlhttp.readyState === 4 && xmlhttp.status === 200) {
                if (templateCache[url] == undefined)
                    templateCache[url] = xmlhttp.responseText;
                
                callback(templateCache[url]);
            }
        }
        xmlhttp.open("GET", url, async);
        //  xmlhttp.overrideMimeType("Access-Control-Allow-Origin: *");
        xmlhttp.send();
    }
    (function (DOMParser) {
        "use strict";
        var DOMParser_proto = DOMParser.prototype
            , real_parseFromString = DOMParser_proto.parseFromString;
        try {
            if ((new DOMParser).parseFromString("", "text/html")) {
                return;
            }
        } catch (ex) {
            console.error(ex);
        }

        DOMParser_proto.parseFromString = function (markup, type) {
            if (/^\s*text\/html\s*(?:;|$)/i.test(type)) {
                var doc = document.implementation.createHTMLDocument("")
                    , doc_elt = doc.documentElement
                    , first_elt;

                doc_elt.innerHTML = markup;
                first_elt = doc_elt.firstElementChild;

                if (doc_elt.childElementCount === 1 && first_elt.localName.toLowerCase() === "html") {
                    doc.replaceChild(first_elt, doc_elt);
                }

                return doc;
            } else {
                return real_parseFromString.apply(this, arguments);
            }
        };
    }(DOMParser));


    HTMLElement.prototype.add = function add(nodes) {
        this.innerHTML += nodes;
    }

    String.prototype.replaceAll = function (search, replacement) {
        var target = this;
        return target.replace(new RegExp(search, 'g'), replacement);
    };
    $.fn.exists = function () {
        return this.length !== 0;
    }
    String.prototype.format = function () {
        var args = arguments;
        return this.replace(/\{\{|\}\}|\{(\d+)\}/g, function (m, n) {
            if (m === "{{") { return "{"; }
            if (m === "}}") { return "}"; }
            return args[n];
        });
    };

    String.prototype.formatTemplate = function () {
        var args = arguments;
        return this.replace(/\{\{|\}\}|\{(\w+)\}/g, function (m, name) {
            if (m === "{{") { return "{"; }
            if (m === "}}") { return "}"; }
            return args[0][name]; //0 one object
        });
    };

    //function sar(selector) {
    //    var nodes = [], nodelist, p;
    //    if (selector) {
    //        nodelist = document.querySelectorAll(selector);
    //        for (var i = 0; i < nodelist.length; i += 1) {
    //            nodes[i] = nodelist[i];
    //        }
    //    }
    //    if (nodes.length > 1)
    //        p = nodes;
    //    else
    //        p = nodes[0];
        
    //    return p;
    //}
    //window.$ = sar;
