#if DEBUG
using Multicad;
using Multicad.DatabaseServices;
using Multicad.Symbols;
using System.Collections.Generic;
using System.Diagnostics;

namespace dRz.SpecSPDS.Cad.Commands.Test
{
    //test скорости сбора свойств
    internal class ExtractAllProperties
    {
        Stopwatch _stw;

        List<McObjectId> McObjectIds;
        List<Dictionary<string, object>> _mprops;
        string _tmrGetProps;
        string _tmrPopsSource;


        /// <summary>
        /// Extracts all propsSource. Все маркеры и все их свойства в список словарей
        /// собирать все свойства в 30 раз медленнее чем выборочно
        /// </summary>
        public void ExtractAllPropertiesGetProps()
        {
            /*
            Найдено всего: 162364 за 00:00:00.3047090
            Включено в набор: 72162 маркеров за 00:00:01.7906102
            С неподходящим именем: 72160
            Без признака включения в спецификацию: 18040
            С некорректными данными: 2
            prop.GetValue() props           162364 in 00:00:35.3129445
            propsSource.GetValueEx props    162364 in 00:00:26.0401367

            propsSource.GetValueEx  ~ в 1,5 раза быстрее чем prop.GetValue()
            ----

            без List<McProperty> props = propsSource.GetPropsMC(); и цикла по props
            ex props
            GetPropsMC Ex props 162364 in 00:00:27.5163433
            Ex props 162364 in 00:00:28.1478998
            ------------
            Ex props 162364 in 00:00:27.8239544
            GetPropsMC Ex props 162364 in 00:00:29.0826902
			
            цикл по propsSource.GetPropsMC() vs propsSource в пределах погрешности


            ----
            пустой цикл
            GetPropsMC props 0 in 00:00:12.9194806
            PropSourse props 0 in 00:00:13.5995537
            ------------
            PropSourse props 0 in 00:00:12.9806616
            GetPropsMC props 0 in 00:00:13.0468145

            с заполнением словарика на 15 сек дольше!!!

            */

            //test prop.Value

            _stw.Restart();

            _mprops = new List<Dictionary<string, object>>();


            foreach (McObjectId mcObjectId in McObjectIds)//по собранным ID маркеров
            {

                //Dictionary<string, object> mprop = new Dictionary<string, object>();

                McUMarker? tempUmark = McObjectManager.GetObject(mcObjectId) as McUMarker;

                if (tempUmark == null)//не маркер
                {
                    continue;
                }

                //список свойств
                McProperties propsSource = McPropertySource.GetPropertySource(tempUmark).ObjectProperties;

                List<McProperty> props = propsSource.GetProps();


                foreach (McProperty prop in /*propsSource*/ props)

                {
                    //mprop.Add(prop.Name, prop.GetValue());
                    //mprop.Add(prop.Name, propsSource.GetValueEx(prop.Name, ""));
                }

                //_mprops.Add(mprop);
            }
            _stw.Stop();
            _tmrGetProps = _stw.Elapsed.ToString();
        }

        public void ExtractAllPropertiesPropsSource()
        {
            //test EX
            _stw.Restart();

            _mprops = new List<Dictionary<string, object>>();

            foreach (McObjectId mcObjectId in McObjectIds)//по собранным ID маркеров
            {

                //Dictionary<string, object> mprop = new Dictionary<string, object>();

                McUMarker? tempUmark = McObjectManager.GetObject(mcObjectId) as McUMarker;

                if (tempUmark == null)//не маркер
                {
                    continue;
                }

                //список свойств
                McProperties propsSource = McPropertySource.GetPropertySource(tempUmark).ObjectProperties;

                //List<McProperty>  props = propsSource.GetPropsMC();


                foreach (McProperty prop in propsSource /*props*/)

                {
                    //самый быстрый, ~ в 1,5 раза быстрее чем 
                    //mprop.Add(prop.Name, propsSource.GetValueEx(prop.Name, ""));

                }

                //_mprops.Add(mprop);
            }

            _stw.Stop();
            _tmrPopsSource = _stw.Elapsed.ToString();
        }

    }
}
#endif