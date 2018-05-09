templateCache = {};
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

String.prototype.formatTemplate = function () {
    var args = arguments;
    return this.replace(/\{\{|\}\}|\{(\w+)\}/g, function (m, name) {
        if (m === "{{") { return "{"; }
        if (m === "}}") { return "}"; }
        return args[0][name]; //0 one object
    });
};

var Templates =  {
    updateValAndData : function(maskSl, key, value) {
        $("#"+maskSl + key).bootstrapSlider('setValue', value);
    },
    callAjax : function(url, callback, async) {
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
        xmlhttp.send();
    },
    blockCardInfo : function (parentContainer, asPatch, object) {
        this.callAjax("templates/cardblock.templ",
                function (result) {
                    if (!asPatch) {
                        parentContainer.append(result.formatTemplate(object));
                        $("[id^=master_flag_").hide();
                    } else {
                        Templates.updateValAndData("sl_v_", object.key, object.VoltageCurrent);
                        Templates.updateValAndData("sl_pl_", object.key, object.PowerLimitCurrent);
                        Templates.updateValAndData("sl_tl_", object.key, object.TempLimitCurrent);
                        Templates.updateValAndData("sl_cc_", object.key, object.CoreClockCurrent);
                        Templates.updateValAndData("sl_mc_", object.key, object.MemoryClockCurrent);
                        Templates.updateValAndData("sl_fs_", object.key, object.FanSpeedCurrent);
                        if (object.IsMaster) {
                            $("[id^=master_flag_").hide();
                            $("#master_flag_" + object.key).show();
                        }
                    }
                    $("[id^=sl_]").bootstrapSlider({ tooltip_position: 'bottom', tooltip: 'show' });
                },
                true);
        
    }
}








