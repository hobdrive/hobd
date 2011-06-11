
[1, 2].each{ |a2|
[1, 2, 3, 4].each{ |a1|

print <<xx

  <parameter id="Oxygen_b#{a2}s#{a1}" display="O2S#{a2}#{a1}" precision="3">
    <description>
      <name>Oxygen b#{a2}s#{a1}</name>      <unit>V</unit>      <description>Oxygen Sensor Output Voltage bank #{a2} sensor #{a1}</description>
    </description>
    <address> <byte>0x#{(0x14+a2*4+a1 - 1-4).to_s(16)}</byte> </address>
    <valuea> 0.005 </valuea>
  </parameter>
  <parameter id="Oxygen_b#{a2}s#{a1}_stft">
    <description>
      <name>Oxygen b#{a2}s#{a1} STFT</name>      <unit>%</unit>      <description>STFT for Oxygen bank #{a2} sensor #{a1}</description>
    </description>
    <address> <byte>0x#{(0x14+a2*4+a1 - 1-4).to_s(16)}</byte> </address>
    <value> (get(1)-128)*100/128 </value>
  </parameter>
xx

}
}


[1, 2, 3, 4].each{ |a1|
  [1, 2].each{ |a2|

print <<xx

  <parameter id="Lambda_b#{a1}s#{a2}" display="LAMBDA#{a1}#{a2}" precision="3">
    <description>
      <name>Lambda b#{a1}s#{a2}</name>      <unit></unit>      <description>Equivalence ratio (Lambda) bank #{a1} sensor #{a2}</description>
    </description>
    <address> <byte>0x#{(0x24+a1*2+a2 - 1-2).to_s(16)}</byte> </address>
    <valueab>2 / 65535</valueab>
  </parameter>
  <parameter id="Oxygen2_b#{a1}s#{a2}" display="O2S#{a1}#{a2}" precision="3">
    <description>
      <name>Oxygen b#{a1}s#{a2}</name>      <unit>V</unit>      <description>Oxygen sensor voltage bank #{a1} sensor #{a2}</description>
    </description>
    <address> <byte>0x#{(0x24+a1*2+a2 - 1-2).to_s(16)}</byte> </address>
    <valuecd> 8 / 65535 </valuecd>
  </parameter>
xx

}
}