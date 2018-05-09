
var Templates = function () {
    var updateValAndData = function(maskSl, key, value) {
        $("#"+maskSl + key).bootstrapSlider('setValue', value);
    } 
    this.blockCardInfo = function (parentContainer, asPatch, object) {
        
            callAjax("Templates\\cardblock.templ",
                function (result) {
                    if (!asPatch) {
                        parentContainer.append(result.formatTemplate(object));
                        $("[id^=sl_]").bootstrapSlider({ tooltip_position: 'bottom' });
                        $("[id^=master_flag_").hide();
                    } else {
                        updateValAndData("sl_v_", object.key, object.VoltageCurrent);
                        updateValAndData("sl_pl_", object.key, object.PowerLimitCurrent);
                        updateValAndData("sl_tl_", object.key, object.TempLimitCurrent);
                        updateValAndData("sl_cc_", object.key, object.CoreClockCurrent);
                        updateValAndData("sl_mc_", object.key, object.MemoryClockCurrent);
                        updateValAndData("sl_fs_", object.key, object.FanSpeedCurrent);
                        if (object.IsMaster) {
                            $("[id^=master_flag_").hide();
                            $("#master_flag_" + object.key).show();
                        }
                    }
                },
                true);
        
    };
};








